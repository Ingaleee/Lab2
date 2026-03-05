const signalR = require("@microsoft/signalr");

const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5086/hubs/orders")
    .build();

connection.on("orderStatusChanged", (event) => {
    console.log("📢 Status changed:", {
        orderId: event.orderId,
        orderNumber: event.orderNumber,
        oldStatus: event.oldStatus,
        newStatus: event.newStatus,
        eventId: event.eventId
    });
});

connection.start()
    .then(() => {
        console.log("✅ Connected to SignalR hub");
        return connection.invoke("JoinOrdersList");
    })
    .then(() => console.log("✅ Joined orders list group"))
    .catch(console.error);

process.on("SIGINT", async () => {
    await connection.stop();
    process.exit(0);
});
