// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener('DOMContentLoaded', async function () {
    const notificationCountElement = document.getElementById('notificationCount');

    try {
        // Pobranie liczby nieprzeczytanych powiadomień po załadowaniu strony
        const countResponse = await fetch('/Notification/GetUnreadNotificationsCount');
        const countData = await countResponse.json();

        // Zaktualizowanie liczby powiadomień
        notificationCountElement.textContent = countData.count;
    } catch (error) {
        console.error('Błąd podczas ładowania liczby nieprzeczytanych powiadomień:', error);
    }
});

document.getElementById('notificationsDropdown').addEventListener('click', async function () {
    const notificationList = document.getElementById('notificationList');
    notificationList.innerHTML = '<li><span class="dropdown-item">Ładowanie...</span></li>';

    try {
        const response = await fetch('/Notification/GetNotifications');
        const data = await response.json();

        if (data.length === 0) {
            notificationList.innerHTML = '<li><span class="dropdown-item">Brak nowych powiadomień</span></li>';
        } else {
            notificationList.innerHTML = '';
            data.forEach(notification => {
                notificationList.innerHTML += `
                       <li class="dropdown-item" id="notification-${notification.id}">
                            <span>${notification.message}</span>
                            <div style="font-size: 0.8em; color: gray;">
                                 ${new Date(notification.createdAt).toLocaleString('pl-PL', { timeZone: 'Europe/Warsaw' })}
                            </div>
                            <button class="btn btn-sm btn-link text-success markAsReadBtn" data-id="${notification.id}">Oznacz jako przeczytane</button>
                        </li>
                    `;
            });

            // Obsługa kliknięcia przycisków „Oznacz jako przeczytane” i aktualizacja powiadomień
            const markAsReadBtns = document.querySelectorAll('.markAsReadBtn');
            markAsReadBtns.forEach(button => {
                button.addEventListener('click', async function () {
                    const notificationId = button.getAttribute('data-id');
                    try {
                        const markAsReadResponse = await fetch(`/Notification/MarkAsRead?id=${notificationId}`, {
                            method: 'POST',
                            headers: {
                                'Content-Type': 'application/x-www-form-urlencoded'
                            }
                        });

                        if (markAsReadResponse.ok) {
                            location.reload(); // Odświeżenie strony
                        } else {
                            alert('Wystąpił problem podczas oznaczania powiadomienia.');
                        }
                    } catch (error) {
                        console.error('Błąd podczas oznaczania powiadomienia:', error);
                    }
                });
            });
        }
    } catch (error) {
        notificationList.innerHTML = '<li><span class="dropdown-item text-danger">Brak nowych powiadomień.</span></li>';
    }
});
