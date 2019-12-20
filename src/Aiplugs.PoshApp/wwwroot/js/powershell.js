const connection = new signalR.HubConnectionBuilder().withUrl("/powershell").withAutomaticReconnect().build();

await connection.start();