window.printerService = {
    // Configuración de la impresora
    printerConfig: {
        name: 'SAT Q22UB',            // Nombre de la impresora como aparece en Windows
        characterSet: 'SPAIN',        // Juego de caracteres español
        width: 40                     // Ancho del papel en caracteres (48 para 80mm)
    },

    // Información de la empresa
    companyInfo: {
        name: "KARU BITES SRL",
        legalName: "KARU BITES SRL",
        taxId: "CED.JUR. 3-102-924380",
        phone: "506 8806 2822",
        address1: "Puntarenas, Golfito, Rio Claro",
        address2: "Frente Cabinas Pérez",
        website: "¡Síguenos en nuestras redes sociales!"
    },

    // Función para formatear el texto al ancho del papel
    formatTextToWidth: function (text, width) {
        if (!text || text.length <= width) return text;
        const words = text.split(' ');
        let result = '';
        let currentLine = '';

        for (const word of words) {
            if ((currentLine + word).length <= width) {
                currentLine += (currentLine ? ' ' : '') + word;
            } else {
                result += currentLine + '\n';
                currentLine = word;
            }
        }

        if (currentLine) {
            result += currentLine;
        }

        return result;
    },

    // Función para centrar un texto
    centerText: function (text, width) {
        if (!text || text.length >= width) return text;
        const spaces = Math.floor((width - text.length) / 2);
        return ' '.repeat(spaces) + text;
    },

    // Función para crear una línea horizontal
    horizontalLine: function (width, char = '=') {
        return char.repeat(width);
    },

    // Función para imprimir un texto alineado a la derecha
    rightAlign: function (text, width) {
        if (!text || text.length >= width) return text;
        const spaces = width - text.length;
        return ' '.repeat(spaces) + text;
    },

    // Función para imprimir un texto con justificación entre etiqueta y valor
    justifyText: function (label, value, width) {
        const spaces = width - label.length - value.length;
        if (spaces <= 0) return label + value;
        return label + ' '.repeat(spaces) + value;
    },

    // Función para imprimir un ticket de compra
    printPaymentReceipt: async function (orderData) {
        try {
            // Verificar si la API de impresión de Windows está disponible
            if (!window.navigator || !window.navigator.clipboard) {
                console.warn("API de impresión no disponible. Usando fallback.");
                return this.printFallback(this.generatePaymentReceiptContent(orderData));
            }

            // Generar el contenido del ticket
            const content = this.generatePaymentReceiptContent(orderData);

            // Si estamos en desarrollo, mostrar en consola
            if (window.location.hostname === 'localhost') {
                console.log("TICKET DE PAGO:\n" + content);
                return await this.printFallback(content);
            }

            // Intentar imprimir usando la API de impresión
            try {
                const printWindow = window.open('', '_blank');
                printWindow.document.write(`<html><head><title>Ticket de Pago</title>
                    <style>
                        body { font-family: monospace; font-size: 12px; }
                        pre { white-space: pre-wrap; }
                    </style>
                </head><body><pre>${content}</pre></body></html>`);
                printWindow.document.close();

                // Especificar la impresora por su nombre
                const printOptions = {
                    printer: this.printerConfig.name
                };

                printWindow.print();
                setTimeout(() => printWindow.close(), 500);
                return true;
            } catch (printError) {
                console.error("Error en la impresión:", printError);
                return this.printFallback(content);
            }
        } catch (error) {
            console.error("Error al imprimir ticket:", error);
            return false;
        }
    },

    // Función para imprimir un ticket de cocina
    printKitchenTicket: async function (orderData) {
        try {
            // Generar el contenido del ticket
            const content = this.generateKitchenTicketContent(orderData);

            // Si estamos en desarrollo, mostrar en consola
            if (window.location.hostname === 'localhost') {
                console.log("TICKET DE COCINA:\n" + content);
                return await this.printFallback(content);
            }

            // Imprimir usando el mismo método que el ticket de pago
            return await this.printPaymentReceipt({
                ...orderData,
                isKitchenTicket: true
            });
        } catch (error) {
            console.error("Error al imprimir ticket de cocina:", error);
            return false;
        }
    },

    // Método alternativo de impresión (fallback)
    printFallback: async function (content) {
        try {
            const printWindow = window.open('', '_blank');
            printWindow.document.write(`<html><head><title>Ticket</title>
                <style>
                    body { font-family: monospace; font-size: 12px; }
                    pre { white-space: pre-wrap; }
                </style>
            </head><body><pre>${content}</pre></body></html>`);
            printWindow.document.close();
            printWindow.print();
            setTimeout(() => printWindow.close(), 500);
            return true;
        } catch (error) {
            console.error("Error en el fallback de impresión:", error);
            alert("No se pudo imprimir el ticket. Por favor, configure una impresora.");
            return false;
        }
    },

    // Generar contenido del ticket de pago
    generatePaymentReceiptContent: function (orderData) {
        const width = this.printerConfig.width;
        let content = "";

        // Encabezado con información comercial
        content += this.centerText(this.companyInfo.name, width) + "\n";
        content += this.centerText(this.companyInfo.taxId, width) + "\n";
        content += this.centerText(this.companyInfo.phone, width) + "\n";
        content += this.centerText(this.companyInfo.address1, width) + "\n";
        content += this.centerText(this.companyInfo.address2, width) + "\n";
        content += this.horizontalLine(width) + "\n";
        content += this.centerText("FACTURA", width) + "\n";
        content += this.horizontalLine(width) + "\n";

        // Información de la orden
        content += this.justifyText("Orden:", orderData.orderNumber, width) + "\n";
        content += this.justifyText("Fecha:", new Date().toLocaleString(), width) + "\n";

        if (orderData.customerName) {
            content += this.justifyText("Cliente:", orderData.customerName, width) + "\n";
        }

        if (orderData.table) {
            content += this.justifyText("Mesa:", orderData.table, width) + "\n";
        } else {
            content += this.justifyText("Tipo:", this.getOrderTypeName(orderData.orderType), width) + "\n";
        }

        content += this.horizontalLine(width) + "\n";
        content += "CANT  DESCRIPCIÓN                PRECIO\n";
        content += this.horizontalLine(width) + "\n";

        // Detalle de productos - ASEGURARSE QUE MUESTRE NOMBRES CORRECTOS
        orderData.items.forEach(item => {
            // Formatear el nombre del producto para asegurar que sea legible
            let productName = item.name || "Producto sin nombre";

            // Limitar longitud para evitar líneas muy largas
            if (productName.length > 25) {
                productName = productName.substring(0, 22) + "...";
            }

            // Formato: cantidad [3 chars] | nombre [25 chars] | precio unitario [8 chars]
            let quantityStr = String(item.quantity).padStart(3, ' ');
            let priceStr = `${item.price.toFixed(2)}`;

            content += `${quantityStr}  ${productName.padEnd(25, ' ')} ${priceStr}\n`;
            content += `      ${this.rightAlign(`${(item.quantity * item.price).toFixed(2)}`, width - 6)}\n`;

            // Añadir elementos del combo si es un combo
            if (item.isCombo && item.comboItems && item.comboItems.length > 0) {
                content += `      Contiene:\n`;
                item.comboItems.forEach(comboItem => {
                    content += `        - ${comboItem.name} x${comboItem.quantity}\n`;

                    // Añadir instrucciones especiales del ítem del combo si existen
                    if (comboItem.specialInstructions) {
                        content += `          Nota: ${comboItem.specialInstructions}\n`;
                    }
                });
            }

            // Añadir personalizaciones si existen
            if (item.customizations && item.customizations.length > 0) {
                item.customizations.forEach(custom => {
                    let customText = `      - ${this.getCustomizationTypeName(custom.type)}: ${custom.name}`;
                    if (custom.quantity > 1) {
                        customText += ` x${custom.quantity}`;
                    }
                    content += customText + "\n";
                });
            }

            if (item.notes && !item.notes.includes(" - ")) { // Evitar imprimir notas que contengan el formato "ID - Nombre"
                content += `      Nota: ${item.notes}\n`;
            }

            content += "\n";
        });

        content += this.horizontalLine(width) + "\n";

        // Resumen de pago con justificación
        content += this.justifyText("Subtotal:", `₡${orderData.subtotal.toFixed(2)}`, width) + "\n";
        content += this.justifyText("IVA (13%):", `₡${orderData.tax.toFixed(2)}`, width) + "\n";

        if (orderData.discount > 0) {
            content += this.justifyText("Descuento:", `-₡${orderData.discount.toFixed(2)}`, width) + "\n";
        }

        content += this.horizontalLine(width) + "\n";
        content += this.justifyText("TOTAL:", `₡${orderData.total.toFixed(2)}`, width) + "\n";
        content += this.horizontalLine(width) + "\n\n";

        // Información de pago
        content += this.justifyText("Forma de pago:", orderData.paymentMethod, width) + "\n";

        // Manejo de pago con múltiples monedas
        if (orderData.paymentMethod === "Efectivo") {
            // Verificar si hay información de moneda
            if (orderData.currency && orderData.currency === "USD") {
                // Pago en dólares
                content += this.justifyText("Recibido:", `$${orderData.amountReceivedOriginal ? orderData.amountReceivedOriginal.toFixed(2) : orderData.amountReceived.toFixed(2)}`, width) + "\n";
                content += this.justifyText("Cambio:", `$${orderData.changeOriginal ? orderData.changeOriginal.toFixed(2) : orderData.change.toFixed(2)}`, width) + "\n";
                content += this.justifyText("Tipo de cambio:", `₡${orderData.exchangeRate.toFixed(2)}`, width) + "\n";
                content += this.justifyText("Total en colones:", `₡${orderData.amountReceived.toFixed(2)}`, width) + "\n";
            } else {
                // Pago en colones (moneda predeterminada)
                content += this.justifyText("Recibido:", `₡${orderData.amountReceivedOriginal ? orderData.amountReceivedOriginal.toFixed(2) : orderData.amountReceived.toFixed(2)}`, width) + "\n";
                content += this.justifyText("Cambio:", `₡${orderData.changeOriginal ? orderData.changeOriginal.toFixed(2) : orderData.change.toFixed(2)}`, width) + "\n";
            }
        }

        if (orderData.referenceNumber) {
            content += this.justifyText("Referencia:", orderData.referenceNumber, width) + "\n";
        }

        content += "\n";
        content += this.centerText("¡GRACIAS POR SU COMPRA!", width) + "\n";
        content += this.centerText(this.companyInfo.website, width) + "\n";
        content += "\n\n\n\n";  // Espacio para corte

        return content;
    },

    // Generar contenido del ticket de cocina
    generateKitchenTicketContent: function (orderData) {
        const width = this.printerConfig.width;
        let content = "";

        // Encabezado con información básica de la empresa
        content += this.centerText(this.companyInfo.name, width) + "\n";
        content += this.horizontalLine(width) + "\n";
        content += this.centerText("*** TICKET DE COCINA ***", width) + "\n";
        content += this.horizontalLine(width) + "\n";

        // Información de la orden
        content += this.justifyText("Orden:", orderData.orderNumber, width) + "\n";
        content += this.justifyText("Fecha:", new Date().toLocaleString(), width) + "\n";

        if (orderData.table) {
            content += this.justifyText("Mesa:", orderData.table, width) + "\n";
        } else {
            content += this.justifyText("Tipo:", this.getOrderTypeName(orderData.orderType), width) + "\n";
        }

        content += this.horizontalLine(width) + "\n\n";
        content += "CANT  DESCRIPCIÓN\n";
        content += this.horizontalLine(width) + "\n";

        // Detalle de productos (más enfocado a la cocina)
        orderData.items.forEach(item => {
            let productName = item.name || "Producto sin nombre";
            let quantityStr = String(item.quantity).padStart(3, ' ');

            content += `${quantityStr}  ${productName}\n`;

            // Añadir elementos del combo si es un combo
            if (item.isCombo && item.comboItems && item.comboItems.length > 0) {
                content += `      Contiene:\n`;
                item.comboItems.forEach(comboItem => {
                    content += `        - ${comboItem.name} x${comboItem.quantity}\n`;

                    // Añadir instrucciones especiales del ítem del combo si existen
                    if (comboItem.specialInstructions) {
                        content += `          Nota: ${comboItem.specialInstructions}\n`;
                    }
                });
            }

            // Añadir personalizaciones si existen
            if (item.customizations && item.customizations.length > 0) {
                item.customizations.forEach(custom => {
                    content += `      ${custom.type}: ${custom.name} x${custom.quantity}\n`;
                });
            }

            // Añadir notas si existen
            if (item.notes) {
                content += `      Notas: ${item.notes}\n`;
            }

            content += "\n";
        });

        content += this.horizontalLine(width) + "\n";

        // Si hay notas generales para la orden
        if (orderData.notes) {
            content += "NOTAS GENERALES:\n";
            content += this.formatTextToWidth(orderData.notes, width) + "\n";
            content += this.horizontalLine(width) + "\n";
        }

        content += this.centerText("PREPARAR CON PRONTITUD", width) + "\n";
        content += "\n\n\n\n";  // Espacio para corte

        return content;
    },

    // Obtener nombre legible del tipo de orden
    getOrderTypeName: function (type) {
        switch (type) {
            case "DineIn": return "En Sitio";
            case "TakeOut": return "Para Llevar";
            case "Delivery": return "Entrega";
            default: return type;
        }
    },

    // Obtener nombre legible del tipo de personalización
    getCustomizationTypeName: function (type) {
        switch (type) {
            case "Add": return "Agregar";
            case "Remove": return "Quitar";
            case "Extra": return "Extra";
            default: return type;
        }
    },

    // Función para imprimir solo ticket de cocina
    printKitchenTicketOnly: async function (orderData) {
        try {
            // Generar el contenido del ticket de cocina
            const content = this.generateKitchenTicketOnlyContent(orderData);

            // Si estamos en desarrollo, mostrar en consola
            if (window.location.hostname === 'localhost') {
                console.log("TICKET DE COCINA (SOLO):\n" + content);
                return true;
            }

            // Intentar imprimir usando la API de impresión
            try {
                const printWindow = window.open('', '_blank');
                printWindow.document.write(`<html><head><title>Ticket de Cocina</title>
                    <style>
                        body { font-family: monospace; font-size: 12px; }
                        pre { white-space: pre-wrap; }
                    </style>
                </head><body><pre>${content}</pre></body></html>`);
                printWindow.document.close();

                // Especificar la impresora por su nombre
                const printOptions = {
                    printer: this.printerConfig.name
                };

                printWindow.print();
                setTimeout(() => printWindow.close(), 500);
                return true;
            } catch (printError) {
                console.error("Error en la impresión del ticket de cocina:", printError);
                return this.printFallback(content);
            }
        } catch (error) {
            console.error("Error al imprimir ticket de cocina:", error);
            return false;
        }
    },

    // Generar contenido del ticket de cocina sin información de pago
    generateKitchenTicketOnlyContent: function (orderData) {
        const width = this.printerConfig.width;
        let content = "";

        // Encabezado con información básica de la empresa
        content += this.centerText(this.companyInfo.name, width) + "\n";
        content += this.horizontalLine(width) + "\n";
        content += this.centerText("*** TICKET DE COCINA ***", width) + "\n";
        content += this.horizontalLine(width) + "\n";

        // Información de la orden
        content += this.justifyText("Orden:", orderData.orderNumber, width) + "\n";
        content += this.justifyText("Fecha:", new Date().toLocaleString(), width) + "\n";

        if (orderData.table) {
            content += this.justifyText("Mesa:", orderData.table, width) + "\n";
        } else {
            content += this.justifyText("Tipo:", this.getOrderTypeName(orderData.orderType), width) + "\n";
        }

        content += this.horizontalLine(width) + "\n\n";
        content += "CANT  DESCRIPCIÓN\n";
        content += this.horizontalLine(width) + "\n";

        // Detalle de productos (más enfocado a la cocina)
        orderData.items.forEach(item => {
            let productName = item.name || "Producto sin nombre";
            let quantityStr = String(item.quantity).padStart(3, ' ');

            content += `${quantityStr}  ${productName}\n`;

            // Añadir personalizaciones si existen
            if (item.customizations && item.customizations.length > 0) {
                item.customizations.forEach(custom => {
                    content += `      ${this.getCustomizationTypeName(custom.type)}: ${custom.name} x${custom.quantity}\n`;
                });
            }

            // Añadir notas si existen
            if (item.notes) {
                content += `      Notas: ${item.notes}\n`;
            }

            content += "\n";
        });

        content += this.horizontalLine(width) + "\n";

        // Si hay notas generales para la orden
        if (orderData.notes) {
            content += "NOTAS GENERALES:\n";
            content += this.formatTextToWidth(orderData.notes, width) + "\n";
            content += this.horizontalLine(width) + "\n";
        }

        content += this.centerText("PREPARAR CON PRONTITUD", width) + "\n";
        content += "\n\n\n\n";  // Espacio para corte

        return content;
    }
};