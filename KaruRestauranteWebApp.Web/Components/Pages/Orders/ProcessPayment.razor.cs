using KaruRestauranteWebApp.Models.Entities.Orders;
using KaruRestauranteWebApp.Models.Entities.Restaurant;
using KaruRestauranteWebApp.Models.Models.Orders;
using KaruRestauranteWebApp.Models.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Radzen;

namespace KaruRestauranteWebApp.Web.Components.Pages.Orders
{
    public partial class ProcessPayment
    {
        [Parameter]
        public int OrderId { get; set; }

        [Inject]
        public required ApiClient ApiClient { get; set; }

        [Inject]
        public required NavigationManager NavigationManager { get; set; }

        [Inject]
        public required NotificationService NotificationService { get; set; }

        [Inject]
        public required DialogService DialogService { get; set; }

        [Inject]
        public required IJSRuntime JSRuntime { get; set; }

        private bool isLoaded;
        private OrderModel order;
        private decimal paidAmount;
        private decimal pendingAmount;
        private List<IngredientModel> ingredients = new();
        private List<FastFoodItemModel> products = new();
        private List<ComboModel> combos = new();

        protected override async Task OnInitializedAsync()
        {
            try
            {
                await LoadData();
                isLoaded = true;
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al cargar los datos: {ex.Message}", 4000);
            }
        }

        //private async Task LoadData()
        //{
        //    // Cargar la orden
        //    var orderResponse = await ApiClient.GetFromJsonAsync<BaseResponseModel>($"api/Order/{OrderId}");
        //    if (orderResponse?.Success == true)
        //    {
        //        order = JsonConvert.DeserializeObject<OrderModel>(orderResponse.Data.ToString());

        //        // Calcular el monto pagado y pendiente
        //        if (order != null)
        //        {
        //            paidAmount = order.Payments?.Sum(p => p.Amount) ?? 0;
        //            pendingAmount = order.TotalAmount - paidAmount;

        //            // Si ya está pagado completamente, actualizar el estado
        //            if (paidAmount >= order.TotalAmount && order.PaymentStatus != "Paid")
        //            {
        //                await UpdateOrderPaymentStatus("Paid");
        //            }
        //            else if (paidAmount > 0 && paidAmount < order.TotalAmount && order.PaymentStatus != "Partially Paid")
        //            {
        //                await UpdateOrderPaymentStatus("Partially Paid");
        //            }
        //        }
        //    }

        //    // Cargar ingredientes para mostrar nombres en las personalizaciones
        //    var ingredientsResponse = await ApiClient.GetFromJsonAsync<BaseResponseModel>("api/Inventory/ingredients");
        //    if (ingredientsResponse?.Success == true)
        //    {
        //        ingredients = JsonConvert.DeserializeObject<List<IngredientModel>>(ingredientsResponse.Data.ToString()) ?? new();
        //    }
        //}

        private async Task LoadData()
        {
            // Cargar la orden (código existente)
            var orderResponse = await ApiClient.GetFromJsonAsync<BaseResponseModel>($"api/Order/{OrderId}");
            if (orderResponse?.Success == true)
            {
                order = JsonConvert.DeserializeObject<OrderModel>(orderResponse.Data.ToString());

                // Calcular el monto pagado y pendiente (código existente)
                if (order != null)
                {
                    paidAmount = order.Payments?.Sum(p => p.Amount) ?? 0;
                    pendingAmount = order.TotalAmount - paidAmount;

                    // Código existente para actualizar estado...
                }
            }

            // Cargar productos para obtener nombres
            var productsResponse = await ApiClient.GetFromJsonAsync<BaseResponseModel>("api/FastFood");
            if (productsResponse?.Success == true)
            {
                products = JsonConvert.DeserializeObject<List<FastFoodItemModel>>(productsResponse.Data.ToString()) ?? new();
            }

            // Cargar combos para obtener nombres
            var combosResponse = await ApiClient.GetFromJsonAsync<BaseResponseModel>("api/Combo");
            if (combosResponse?.Success == true)
            {
                combos = JsonConvert.DeserializeObject<List<ComboModel>>(combosResponse.Data.ToString()) ?? new();
            }

            // Cargar ingredientes (código existente)
            var ingredientsResponse = await ApiClient.GetFromJsonAsync<BaseResponseModel>("api/Inventory/ingredients");
            if (ingredientsResponse?.Success == true)
            {
                ingredients = JsonConvert.DeserializeObject<List<IngredientModel>>(ingredientsResponse.Data.ToString()) ?? new();
            }
        }

        private async Task UpdateOrderPaymentStatus(string status)
        {
            try
            {
                var response = await ApiClient.PatchAsync<BaseResponseModel>($"api/Order/{order.ID}/status/{status}");
                if (response?.Success == true)
                {
                    order.PaymentStatus = status;
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Warning,
                    "Advertencia", $"No se pudo actualizar el estado de pago: {ex.Message}", 3000);
            }
        }

        private async Task ProcessPaymentMethods()
        {
            try
            {
                if (order == null)
                    return;

                // Mostrar diálogo de pago
                var paymentResult = await DialogService.OpenAsync<PaymentProcessDialog>("Procesar Pago",
                    new Dictionary<string, object>
                    {
                        { "TotalAmount", pendingAmount }
                    },
                    new DialogOptions
                    {
                        Width = "700px",
                        Height = "auto",
                        CloseDialogOnOverlayClick = false
                    });

                if (paymentResult == null || !(paymentResult is PaymentProcessDialog.PaymentResult))
                {
                    // El usuario canceló el pago
                    return;
                }

                var paymentInfo = (PaymentProcessDialog.PaymentResult)paymentResult;

                // Registrar pago
                var paymentDto = paymentInfo.PaymentInfo;
                paymentDto.OrderID = order.ID;

                var paymentResponse = await ApiClient.PostAsync<BaseResponseModel, PaymentDTO>(
                    $"api/Order/{order.ID}/payments", paymentDto);

                if (paymentResponse?.Success == true)
                {
                    // Preparar datos para impresión
                    await PrintReceiptData(paymentInfo);

                    NotificationService.Notify(NotificationSeverity.Success,
                        "Éxito", "Pago registrado exitosamente", 4000);

                    // Recargar los datos para mostrar el nuevo pago
                    await LoadData();
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error,
                        "Error", paymentResponse?.ErrorMessage ?? "Error al registrar el pago", 4000);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al procesar el pago: {ex.Message}", 4000);
            }
        }

        private async Task PrintReceipt()
        {
            if (order == null || order.PaymentStatus != "Paid")
                return;

            try
            {
                // Preparar datos para impresión sin información de pago específica
                await PrintReceiptData(null);

                NotificationService.Notify(NotificationSeverity.Success,
                    "Éxito", "Recibo enviado a impresión", 2000);
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Error", $"Error al imprimir recibo: {ex.Message}", 4000);
            }
        }

        private async Task PrintReceiptData(PaymentProcessDialog.PaymentResult paymentInfo = null)
        {
            // Encontrar el último pago si no se especifica uno
            PaymentModel lastPayment = null;
            if (paymentInfo == null && order.Payments.Any())
            {
                lastPayment = order.Payments.OrderByDescending(p => p.PaymentDate).First();
            }

            // Crear datos para impresión
            var printData = new
            {
                orderNumber = order.OrderNumber,
                customerName = order.Customer?.Name ?? "Cliente General",
                table = order.Table?.TableNumber.ToString() ?? "",
                orderType = order.OrderType,
                items = order.OrderDetails.Select(d => new
                {
                    name = GetItemName(d),
                    quantity = d.Quantity,
                    price = d.UnitPrice,
                    notes = d.Notes,
                    customizations = d.Customizations.Select(c => new
                    {
                        type = c.CustomizationType,
                        name = ingredients.FirstOrDefault(i => i.ID == c.IngredientID)?.Name ?? $"Ingrediente #{c.IngredientID}",
                        quantity = c.Quantity
                    }).ToList()
                }).ToList(),
                subtotal = order.TotalAmount - order.TaxAmount + order.DiscountAmount,
                tax = order.TaxAmount,
                discount = order.DiscountAmount,
                total = order.TotalAmount,
                paymentMethod = paymentInfo != null ?
                    GetPaymentMethodName(paymentInfo.PaymentInfo.PaymentMethod) :
                    (lastPayment != null ? GetPaymentMethodName(lastPayment.PaymentMethod) : "Varios"),
                // Información de pago (si está disponible)
                amountReceived = paymentInfo?.AmountReceived ?? lastPayment?.Amount ?? order.TotalAmount,
                change = paymentInfo?.Change ?? 0,
                currency = paymentInfo?.Currency ?? "CRC",
                exchangeRate = paymentInfo?.ExchangeRate ?? 1,
                amountReceivedOriginal = paymentInfo?.AmountReceivedOriginal ?? 0,
                changeOriginal = paymentInfo?.ChangeOriginal ?? 0,
                referenceNumber = paymentInfo?.PaymentInfo.ReferenceNumber ?? lastPayment?.ReferenceNumber ?? "",
                notes = order.Notes
            };

            // Imprimir recibo de pago
            await JSRuntime.InvokeVoidAsync("printerService.printPaymentReceipt", printData);
        }

        private void GoBack()
        {
            NavigationManager.NavigateTo("/orders");
        }

        private string GetOrderTypeName(string type)
        {
            return type switch
            {
                "DineIn" => "En Sitio",
                "TakeOut" => "Para Llevar",
                "Delivery" => "A Domicilio",
                _ => type
            };
        }

        private string GetPaymentMethodName(string method)
        {
            return method switch
            {
                "Cash" => "Efectivo",
                "CreditCard" => "Tarjeta de Crédito",
                "DebitCard" => "Tarjeta de Débito",
                "Transfer" => "Transferencia",
                "SIMPE" => "SIMPE Móvil",
                "Other" => "Otro",
                _ => method
            };
        }

        private string GetPaymentStatusName(string status)
        {
            return status switch
            {
                "Pending" => "Pendiente",
                "Partially Paid" => "Pago Parcial",
                "Paid" => "Pagado",
                "Cancelled" => "Cancelado",
                _ => status
            };
        }

        private BadgeStyle GetPaymentStatusBadgeStyle(string status)
        {
            return status switch
            {
                "Pending" => BadgeStyle.Warning,
                "Partially Paid" => BadgeStyle.Info,
                "Paid" => BadgeStyle.Success,
                "Cancelled" => BadgeStyle.Danger,
                _ => BadgeStyle.Light
            };
        }

        private string GetItemName(OrderDetailModel detail)
        {
            // En lugar de solo devolver el ID y el tipo, devolver el nombre real
            // Si el detail tiene un nombre, usarlo directamente
            //if (!string.IsNullOrEmpty(detail.ItemName))
            //{
            //    return detail.ItemName;
            //}

            // Si no tiene nombre, intentar obtenerlo del contexto
            if (detail.ItemType == "Product")
            {
                // Si ya tenemos productos cargados, buscar en la lista
                var product = products.FirstOrDefault(p => p.ID == detail.ItemID);
                if (product != null)
                {
                    return product.Name;
                }

                // Si no, devolver algo genérico pero más informativo
                return $"Producto #{detail.ItemID}";
            }
            else if (detail.ItemType == "Combo")
            {
                // Si ya tenemos combos cargados, buscar en la lista
                var combo = combos.FirstOrDefault(c => c.ID == detail.ItemID);
                if (combo != null)
                {
                    return combo.Name;
                }

                // Si no, devolver algo genérico pero más informativo
                return $"Combo #{detail.ItemID}";
            }

            // Por defecto, devolver algo genérico
            return $"Item {detail.ItemType} #{detail.ItemID}";
        }

        private BadgeStyle GetCustomizationBadgeStyle(string type)
        {
            return type switch
            {
                "Add" => BadgeStyle.Success,
                "Remove" => BadgeStyle.Danger,
                "Extra" => BadgeStyle.Warning,
                _ => BadgeStyle.Light
            };
        }
    }
}
