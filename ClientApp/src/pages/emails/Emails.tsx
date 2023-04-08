import React, {useEffect, useState} from 'react';
import {host} from "../../envt";

import styles from './Email.module.scss'
import {Skeleton} from "primereact/skeleton";
import EmailPreview from "../../components/EmailPreview/EmailPreview";
import {ScrollTop} from "primereact/scrolltop";
import {MegaMenu} from "primereact/megamenu";
import {MenuItem, MenuItemCommandEvent} from "primereact/menuitem";
import {PrimeIcons} from "primereact/api";
import {ScrollPanel} from "primereact/scrollpanel";
import {Dialog} from "primereact/dialog";
import SendEmailDialog from "./Dialogs/SendEmailDialog";
import SendVerifyEds from "./Dialogs/SendVerifyEds";

interface IEmailMessage {
    id: string;
    from: string;
    title: string;
    countAttachments: number;
}

const Emails = () => {
    const [messages, setMessages] = useState<IEmailMessage[]>([]);
    const [loader, setLoader] = useState<boolean>(true)
    const [sendEmailModal, openSetEmailModal] = useState<boolean>(false)

    const [verifyMessageModal, openVerifyMessageModal] = useState<boolean>(false)

    useEffect(() => {
        (async function () {
            const response = await fetch(`${host}/email/messages`, {
                    credentials: 'include'
                }
            )
            if (response.ok) {
                const userMessages: IEmailMessage[] = await response.json();
                setMessages(userMessages)
                setLoader(false)
            }
        })()
    }, [messages])

    const items: MenuItem[] = [
        {
            label: 'Отправить сообщение с ЭЦП',
            icon: PrimeIcons.SEND,
            command(event: MenuItemCommandEvent) {
                openSetEmailModal(true)
            }
        },
        {
            label: 'Проверить открепленную подпись',
            icon: PrimeIcons.UNLOCK,
            command(event: MenuItemCommandEvent) {
                openVerifyMessageModal(true)
            }
        },
        {
            label: 'Обновить входящую почту',
            icon: PrimeIcons.REFRESH,
            command(event: MenuItemCommandEvent) {
                setLoader(true)
                setMessages([])
         }},
         {
            label: 'Выйти из уч. записи',
            icon: PrimeIcons.SIGN_OUT,
            command(event: MenuItemCommandEvent) {
               window.location.href = host + '/email/logout'
            }
        }
    ];
    return (
        <>
            <SendEmailDialog visible={sendEmailModal} onHide={(visible) => openSetEmailModal(visible)}/>
            <SendVerifyEds visible={verifyMessageModal} onHide={(visible) => openVerifyMessageModal(visible)}/>
            <MegaMenu model={items} breakpoint="960px"/>
            <ScrollPanel className={styles.scrollPanel} style={{width: '100%', height: '750px'}}>
                <div className={styles.emailContainer}>
                    {loader ? Array.from({length: 5}).map(skeleton => {
                        return (<Skeleton width={'50rem'} height="4rem"></Skeleton>)

                    }) : (messages.length !== 0 && messages.map(message => {
                        return (<EmailPreview id={message.id} from={message.from} title={message.title}
                                              countAttachments={message.countAttachments}/>)
                    }))}
                </div>
                <ScrollTop/>
            </ScrollPanel>
        </>
    );
};

export default Emails;