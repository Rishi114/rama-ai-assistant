const { contextBridge, ipcRenderer } = require('electron');

contextBridge.exposeInMainWorld('electronAPI', {
  minimize: () => ipcRenderer.send('window-minimize'),
  maximize: () => ipcRenderer.send('window-maximize'),
  close: () => ipcRenderer.send('window-close'),
  // For sending messages to Rama brain
  sendToBrain: (message) => ipcRenderer.invoke('brain-message', message),
  // For receiving brain responses
  onBrainResponse: (callback) => ipcRenderer.on('brain-response', (event, response) => callback(response))
});