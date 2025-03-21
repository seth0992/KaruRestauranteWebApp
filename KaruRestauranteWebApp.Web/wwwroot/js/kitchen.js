// Función para hacer scroll a un elemento específico
function scrollToElement(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
}

// Función para reproducir sonido de notificación
function playNotificationSound() {
    const audio = new Audio('/sounds/notification.wav');
    audio.play().catch(error => console.log('Error reproduciendo sonido:', error));
}

// Agregar esto al archivo wwwroot/js/kitchen.js
window.scrollToElement = function (elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        // Primero, quitar cualquier resaltado existente
        const highlighted = document.querySelectorAll('.highlight-element');
        highlighted.forEach(el => el.classList.remove('highlight-element'));

        // Agregar clase para resaltar el elemento
        element.classList.add('highlight-element');

        // Hacer scroll hacia el elemento
        element.scrollIntoView({ behavior: 'smooth', block: 'center' });

        // Quitar la clase después de unos segundos
        setTimeout(() => {
            element.classList.remove('highlight-element');
        }, 3000);
    }
};