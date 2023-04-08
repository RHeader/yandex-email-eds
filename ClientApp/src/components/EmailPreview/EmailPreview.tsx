import React, {FC, useRef, useState} from 'react';
import styles from './EmailPreview.module.scss'
import {Dialog} from "primereact/dialog";
import {host} from "../../envt";
import {Skeleton} from "primereact/skeleton";
import {PrimeIcons} from "primereact/api";
import DownloadableIcon from "../DownloadableIcon/DownloadableIcon";
import {FileUpload, FileUploadHandlerEvent, FileUploadSelectEvent} from "primereact/fileupload";
import {Toast} from "primereact/toast";

interface IEmailPreviewProps {
    id: string;
    from: string;
    title: string;
    countAttachments: number;
}

interface IAttachments {
    base64File: string;
    fileName: string;
    contentType: string;
}

interface IEmailProps {
    id: string;
    from: string;
    title: string;
    body: string;
    attachments: IAttachments[];
}
function getExtension(filename: string) {
    return filename.split('.').pop()
}

const EmailPreview: FC<IEmailPreviewProps> = ({...props}) => {
    const [visibleMessage, setVisibleMessage] = useState<boolean>(false)
    const [loader, setLoader] = useState<boolean>(false)
    const [message, setMessage] = useState<IEmailProps | null>(null);
    const fileRef = useRef<FileUpload>(null);


    const toast = useRef<Toast>(null);

    const onShowDialog = async (id: string) => {
        if (!message) {
            setLoader(true)
            const response = await fetch(`${host}/email/messages/${id}`, {
                    credentials: 'include'
                }
            )
            if (response.ok) {
                const userMessages: IEmailProps = await response.json();
                setMessage(userMessages)
                setLoader(false)
            }
        }
    }


    const goVerify = async (event: FileUploadHandlerEvent) =>{
            const form = new FormData();
            const crt = event.files.find(x => getExtension(x.name) == 'crt')
            if (!crt) {
                alert('Нет публичного сертификата')
                return
            }
            form.append('publicCertificate', crt)

            form.append('id', props.id)

            const response = await fetch(`${host}/email/verify/body`,{
                method: 'POST',
                credentials: 'include',
                body:form
            })
            fileRef?.current?.clear();
            if(response.ok){
                const body = await response.json()

                if(body.verify){
                    toast.current?.show({severity:'success', summary: 'Success', detail:'Верификация сообщения  успешна', sticky:true});
                }else{
                    toast.current?.show({severity:'error', summary: 'Error', detail:'Верификация сообщения не успешна', sticky:true});
                }
                return
            }
            alert('Ошибка верификации подписи')
        }



    const notifyAddFile = (event : FileUploadSelectEvent) =>{
        toast.current?.show({
            severity: 'success',
            summary: 'Успешно',
            detail: 'Публичный ключ добавлен',
            sticky: true
        });
    }



    return (
        <>
            <Dialog onShow={() => onShowDialog(props.id)} header={props.title} visible={visibleMessage}
                    onHide={() => setVisibleMessage(false)}>
                <Toast ref={toast}/>
                <div className={styles.emailBox}>
                    {loader ? <Skeleton width="50rem" height="5rem"></Skeleton> :
                        (message &&
                            <>
                                <div className={styles.from}><i
                                    className={PrimeIcons.USER}></i><span>{message?.from} </span>
                                    <FileUpload
                                        ref={fileRef}
                                        mode={'basic'}
                                        className={styles.uploader}
                                        customUpload={true}
                                        onBeforeSelect={notifyAddFile}
                                        uploadHandler={goVerify}
                                        chooseLabel={'Выберите публичный ключ'}
                                        emptyTemplate={<p className="m-0">Загрузите сюда ваш сертификат</p>}
                                        maxFileSize={1000000}
                                    />

                                </div>
                                <div className={styles.body} dangerouslySetInnerHTML={{__html: message.body}}></div>
                                <div
                                    className={styles.attachmentsFile}> {message.attachments.length > 0
                                    && message.attachments.map(attachment => {
                                    return (<DownloadableIcon base64={attachment.base64File}
                                                              contentType={attachment.contentType}
                                                              filename={attachment.fileName}/>)
                                })} </div>
                            </>
                        )
                    }
                </div>

            </Dialog>
            <div className={styles.emailPreviewBox} onClick={() => setVisibleMessage(true)}>
                <div className={styles.info}>
                    <span className={styles.from}>{props.from}</span>
                    <span className={styles.title}>{props.title}</span>
                </div>
                <span
                    className={styles.attachments}>Файлы : {props.countAttachments > 0 ? `${props.countAttachments}` : `нет`}</span>
            </div>
        </>
    );
};

export default EmailPreview;