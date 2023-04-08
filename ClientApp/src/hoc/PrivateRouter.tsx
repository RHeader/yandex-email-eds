import React, {PropsWithChildren, useEffect, useState} from 'react';
import {useNavigate} from "react-router-dom";
import {host} from "../envt";
import {requestWithAuth} from "../helpers";


const serverRedirectManager = (res: Response): Promise<Response> => new Promise((resolve, reject) => {
    if (res.redirected) {
        window.location.href = res.url
    }
    if (res.status == 401) {
        window.location.href = '/'
    }
    resolve(res)
})

const PrivateRouter = ({children}:PropsWithChildren) => {
    const [isAuth, setIsAuth] = useState(false)

    const navigate = useNavigate();

    useEffect(() => {
        if (isAuth) {
            return
        }
        (async function (){
            try {
                const response = await fetch(`${host}/email/authenticated`, {
                    method: "GET",
                    redirect: 'follow',
                    credentials: 'include'
                });

                await serverRedirectManager(response);

                const user = await response.json();

                setIsAuth(true);
            } catch (err) {
                alert(`Аутентификация не успешна: ${err}`);
            }
        })()
    }, [isAuth])

    if (!isAuth) {
        navigate('/')
        return null
    }


    return (
        <>
            {children}
        </>
    );
};

export default PrivateRouter;