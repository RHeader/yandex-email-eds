import React, {useState} from 'react';

import styles from './Login.module.scss'
import {InputText} from "primereact/inputtext";
import {Password} from "primereact/password";
import {PrimeIcons} from "primereact/api";
import {Button} from "primereact/button";
import {host} from "../../envt";
import {useNavigate} from "react-router-dom";
import {MegaMenu} from "primereact/megamenu";

const Login = () => {
    const [login, setLogin] = useState<string>('Identity.osp7@yandex.ru')
    const [password, setPassword] = useState<string>('rninxqvuxwrhipoc')

    const [notValidRequest, setNotValidRequest] = useState<boolean>(false)

    const navigate = useNavigate();

    const goLogin =  async () => {
        const response = await fetch(`${host}/email/login`,{
            method: 'POST',
            headers: {
                "Content-Type": "application/json",
            },
            credentials: 'include',
            body:JSON.stringify({
                email: login,
                password:password
            })
        })

        if(response.ok){
            navigate('/emails')
            return
        }
        setNotValidRequest(true)

    }


    return (
        <div className={styles.containerLogin}>
              <span className="p-input-icon-left">
                <i className={PrimeIcons.USER} />
                <InputText className={notValidRequest?styles.invalid: ''} placeholder="Почта" value={login}  onChange={(e) => setLogin(e.target.value)} />
            </span>
            <Password
                inputClassName={notValidRequest?styles.invalid: ''}
                value={password}
                      placeholder={"Пароль от почтового клиента"}
                      onChange={(e) => setPassword(e.target.value)}
                      feedback={false} />
            <Button label="Войти" onClick={goLogin} />
        </div>
    );
};

export default Login;