import React from 'react';

import styles from './Header.module.scss'
import gmail from '../../assets/email.png'

const Header = () => {
    return (
        <div className={styles.centeredBox}>
            <div className={styles.borderHeaderBox}>
                <h2 className={styles.title}>Почтовый клиент Yandex</h2>
                <div className={styles.logo}>
                    <img src={gmail} alt={'gmail'}/>
                </div>
            </div>
        </div>
    );
};

export default Header;