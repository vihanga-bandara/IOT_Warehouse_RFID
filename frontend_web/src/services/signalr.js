import * as SignalR from '@microsoft/signalr'

let connection = null

const getHubUrl = () => {
  // Derive the hub base from the API URL so it works in dev and prod
  const apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:5218/api'
  const baseUrl = apiUrl.replace(/\/api\/?$/, '')
  const isHttps = baseUrl.startsWith('https://')
  const hubBase = baseUrl.replace(/^https?:\/\//, isHttps ? 'wss://' : 'ws://')
  return `${hubBase}/hubs/kiosk`
}

export const initSignalR = async (token) => {
  const hubUrl = getHubUrl()

  connection = new SignalR.HubConnectionBuilder()
    .withUrl(hubUrl, {
      accessTokenFactory: () => token,
      skipNegotiation: true,
      transport: SignalR.HttpTransportType.WebSockets
    })
    .withAutomaticReconnect()
    .build()

  try {
    await connection.start()
    console.log('SignalR connected to', hubUrl)
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

export const joinScannerGroup = async (deviceId) => {
  if (connection && deviceId) {
    try {
      await connection.invoke('JoinScannerGroup', deviceId)
      console.log('Joined scanner group', deviceId)
    } catch (err) {
      console.error('Failed to join scanner group', deviceId, err)
    }
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
   joinScannerGroup,
  closeSignalR
}
