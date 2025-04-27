document.addEventListener('DOMContentLoaded', function () {

    // أخذ اسم المستخدم وإيميله
    var userName = prompt("Enter your name:");
    var userEmail = prompt("Enter your email:");

    // عناصر الصفحة
    var messageInput = document.getElementById("teamMessageInp");
    var sendButton = document.getElementById("sendTeamMessageBtn");
    var conversationList = document.getElementById("teamConversationUL");

    // إنشاء اتصال مع الـ Hub
    var proxyConnection = new signalR.HubConnectionBuilder()
        .withUrl("/Hubs/ChatHub")
        .build();

    // بدء الاتصال
    proxyConnection.start().then(function () {

        // الانضمام تلقائيًا إلى جروب الفريق
        proxyConnection.invoke("JoinTeamGroup", userEmail);

        // عند الضغط على زر إرسال رسالة
        sendButton.addEventListener('click', function (e) {
            e.preventDefault();

            var message = messageInput.value.trim();
            if (message) {
                proxyConnection.invoke("SendMessageToTeam", userName, userEmail, message);
                messageInput.value = ""; // تفريغ الحقل بعد الإرسال
            }
        });

    }).catch(function (error) {
        console.error('Connection error: ', error.toString());
    });

    // استقبال الرسائل القادمة من الفريق
    proxyConnection.on("ReceiveTeamMessage", function (sender, message) {
        var listItem = document.createElement('li');
        listItem.innerHTML = `<strong>${sender}:</strong> ${message}`;
        conversationList.appendChild(listItem);
    });

});
