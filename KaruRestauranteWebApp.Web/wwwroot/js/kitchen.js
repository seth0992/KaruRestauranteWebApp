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