import React from 'react'
import ReactDOM from 'react-dom/client'
import App from './App'

//theme
import "primereact/resources/themes/lara-light-indigo/theme.css";

//core
import "primereact/resources/primereact.min.css";

//icons
import "primeicons/primeicons.css";


import '../styles/main.scss'

ReactDOM.createRoot(document.getElementById('root') as HTMLElement).render(
    <App />
)
