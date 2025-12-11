import * as SignalR from '@microsoft/signalr'

let connection = null
let reconnectTimeoutId = null

const getHubUrl = () => {
  // Derive the hub base from the API URL so it works in dev and prod
  const apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:5218/api'
  const baseUrl = apiUrl.replace(/\/api\/?$/, '')
  const isHttps = baseUrl.startsWith('https://')
  const hubBase = baseUrl.replace(/^https?:\/\//, isHttps ? 'wss://' : 'ws://')
  return `${hubBase}/hubs/kiosk`
}

export const initSignalR = async (token) => {
  if (connection) {
    // If there's an existing connection, don't create another one.
    return
  }

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
    // Let withAutomaticReconnect handle retries while the kiosk is mounted.
    // We avoid our own manual retry loop so that navigating away/logging out
    // and calling closeSignalR() actually stops connection attempts.
  }
}

export const onCartUpdated = (callback) => {
  if (connection) {
    connection.on('CartUpdated', callback)
  }
}

export const joinScannerGroup = async (deviceId) => {
  if (!connection || !deviceId) {
    return
  }

  // Only attempt to join when the connection is actually established.
  if (connection.state !== SignalR.HubConnectionState.Connected) {
    console.warn('Cannot join scanner group, SignalR not connected yet')
    return
  }

  try {
    await connection.invoke('JoinScannerGroup', deviceId)
    console.log('Joined scanner group', deviceId)
  } catch (err) {
    console.error('Failed to join scanner group', deviceId, err)
  }
}

export const closeSignalR = () => {
  if (connection) {
    if (reconnectTimeoutId) {
      clearTimeout(reconnectTimeoutId)
      reconnectTimeoutId = null
    }

    connection.stop()
    connection = null
  }
}

export default {
  initSignalR,
  onCartUpdated,
   joinScannerGroup,
  closeSignalR
}
