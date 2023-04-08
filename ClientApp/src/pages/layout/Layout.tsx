import React from 'react';
import Header from "../header/Header";

import {Outlet} from 'react-router-dom'

import styles from './Layout.module.scss'

const Layout = () => {
    return (
        <main>
            <Header/>
            <div className={styles.centeredPageBody}>
                <Outlet/>
            </div>
        </main>
    );
};

export default Layout;