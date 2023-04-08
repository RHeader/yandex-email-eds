import { useState } from 'react'
import reactLogo from './assets/react.svg'
import {createBrowserRouter} from "react-router-dom";
import Emails from "../pages/emails/Emails";
import Login from "../pages/login/Login";
import {
  RouterProvider,
} from "react-router-dom";
import Layout from "../pages/layout/Layout";
import PrivateRouter from "../hoc/PrivateRouter";

const router = createBrowserRouter([
  {
    path: "/",
    element: <Layout/>,
    children:[
      {
        index: true,
        element: <Login/>,
      },
      {
        path: '/emails',
        element: <PrivateRouter><Emails/></PrivateRouter> ,
      }
    ]
  }
]);


function App() {

  return (
      <RouterProvider router={router}
      />
  )

}

export default App
