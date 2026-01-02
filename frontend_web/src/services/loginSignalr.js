import * as SignalR from '@microsoft/signalr'

let loginConnection = null

const getLoginHubUrl = () => {
  // Derive the hub base from the API URL so it works in dev and prod
  const apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:5218/api'
  const baseUrl = apiUrl.replace(/\/api\/?$/, '')
  const isHttps = baseUrl.startsWith('https://')
  const hubBase = baseUrl.replace(/^https?:\/\//, isHttps ? 'wss://' : 'ws://')
  return `${hubBase}/hubs/login`
}

export const initLoginSignalR = async () => {
  if (loginConnection) {
    return loginConnection
  }

  const hubUrl = getLoginHubUrl()

  loginConnection = new SignalR.HubConnectionBuilder()
    .withUrl(hubUrl, {
      skipNegotiation: true,
      transport: SignalR.HttpTransportType.WebSockets
    })
    .withAutomaticReconnect()
    .build()

  try {
    await loginConnection.start()
    console.log('Login SignalR connected to', hubUrl)
    return loginConnection
  } catch (err) {
    console.error('Login SignalR connection failed:', err)
    throw err
  }
}

export const joinScannerLoginGroup = async (deviceId) => {
  if (!loginConnection || !deviceId) {
    return
  }

  if (loginConnection.state !== SignalR.HubConnectionState.Connected) {
    console.warn('Cannot join scanner login group, SignalR not connected yet')
    return
  }

  try {
    await loginConnection.invoke('JoinScannerGroupByDeviceId', deviceId)
    console.log('Joined scanner login group for device', deviceId)
  } catch (err) {
    console.error('Failed to join scanner login group', deviceId, err)
  }
}

export const onRfidLoginSuccess = (callback) => {
  if (loginConnection) {
    loginConnection.on('RfidLoginSuccess', callback)
  }
}

export const onLoginFailed = (callback) => {
  if (loginConnection) {
    loginConnection.on('LoginFailed', callback)
  }
}

export const offRfidLoginSuccess = () => {
  if (loginConnection) {
    loginConnection.off('RfidLoginSuccess')
  }
}

export const offLoginFailed = () => {
  if (loginConnection) {
    loginConnection.off('LoginFailed')
  }
}

export const closeLoginSignalR = () => {
  if (loginConnection) {
    loginConnection.stop()
    loginConnection = null
  }
}

export default {
  initLoginSignalR,
  joinScannerLoginGroup,
  onRfidLoginSuccess,
  onLoginFailed,
  offRfidLoginSuccess,
  offLoginFailed,
  closeLoginSignalR
}
