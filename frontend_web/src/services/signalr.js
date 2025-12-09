import * as SignalR from '@microsoft/signalr'

let connection = null

export const initSignalR = async (token) => {
  connection = new SignalR.HubConnectionBuilder()
    .withUrl('http://localhost:5000/hubs/kiosk', {
      accessTokenFactory: () => token,
      skipNegotiation: true,
      transport: SignalR.HttpTransportType.WebSockets
    })
    .withAutomaticReconnect()
    .build()

  try {
    await connection.start()
    console.log('SignalR connected')
  } catch (err) {
    console.error('SignalR connection failed:', err)
    setTimeout(() => initSignalR(token), 5000)
  }
}

export const onCartUpdated = (callback) => {
  if (connection) {
    connection.on('CartUpdated', callback)
  }
}

export const closeSignalR = () => {
  if (connection) {
    connection.stop()
  }
}

export default {
  initSignalR,
  onCartUpdated,
  closeSignalR
}
