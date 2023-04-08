import React, {FC, useRef} from 'react';
import {Dialog} from "primereact/dialog";
import {FileUpload, FileUploadHandlerEvent} from "primereact/fileupload";
import {host} from "../../../envt";
import {useForm, Controller} from "react-hook-form";
import {Button} from "primereact/button";
import {Chips, ChipsChangeEvent} from "primereact/chips";
import {InputText} from "primereact/inputtext";
import {Editor} from "primereact/editor";

import styles from './DialogStyles/SendEmailDialog.module.scss'
import {Toast} from "primereact/toast";
import {Checkbox} from "primereact/checkbox";

interface ISendEmailDialog {
    visible: boolean;
    onHide: (visible: boolean) => void;
}

type ISendEmailForm = {
    to: string[];
    displayName: string;
    subject: string;
    body: string;
    signatureCertificate: File | null;
    attachments: File[] | null;
    signBody:boolean;
}

const SendEmailDialog: FC<ISendEmailDialog> = ({visible, onHide}) => {

    const {register, handleSubmit, watch, formState: {errors}, control} = useForm<ISendEmailForm>();
    const toast = useRef<Toast>(null);


    const handleSendEmail = async (data: ISendEmailForm) => {

        const formData = new FormData();

        if (data.attachments && data.attachments.length > 0) {
            for (let i = 0; i < data.attachments.length; i++) {
                console.log(data.attachments[i])
                formData.append("attachments", data.attachments[i]);
            }
        }
        if (data.to && data.to.length > 0) {
            for (let i = 0; i < data.to.length; i++) {
                formData.append("to[]", data.to[i]);
            }
        }

        formData.append("DisplayName", data.displayName);
        formData.append("Subject", data.subject);
        formData.append("body", data.body);
        formData.append("signBody", JSON.stringify(data.signBody))

        if(data.signatureCertificate)
        formData.append("signatureCertificate", data.signatureCertificate);


        const response = await fetch(`${host}/email/send`, {
            method: 'POST',
            credentials: 'include',
            body: formData
        })

        if (response.ok) {
            toast.current?.show({
                severity: 'success',
                summary: 'Success',
                detail: 'Сообщение отправлено',
                sticky: true
            });
            onHide(false)
            return
        }
        toast.current?.show({
            severity: 'error',
            summary: 'Error',
            detail: 'Сообщение не было отправлено',
            sticky: true
        });
    }

    return (
        <>
            <Toast ref={toast}/>
            <Dialog header={'Отправка сообщения с ЭЦП'} visible={visible}
                    onHide={() => onHide(false)}>
                <form onSubmit={handleSubmit(handleSendEmail)} className={styles.containerEmailFormSender}>
                    <Controller
                        name={'to'}
                        control={control}
                        render={({field: {onChange, value}}) => (

                            <div className={styles.inputStyle}>
                                <label htmlFor="chip">Получатели</label>
                                <Chips aria-describedby="chip-help" id="chip" value={value}
                                       onChange={(e: ChipsChangeEvent) => onChange(e.value)}/>
                                <small id="chip-help">
                                    Введите сюда список получателей, нажатие enter завершает ввод
                                </small>
                            </div>

                        )}
                    />
                    <Controller
                        name={'displayName'}
                        control={control}
                        render={({field: {onChange, value}}) => (
                            <div className={styles.inputStyle}>
                                <label htmlFor="username">Отображаемое имя</label>
                                <InputText value={value} onChange={(e) => onChange(e.target.value)}/>
                                <small id="username-help">
                                    Введите свое имя
                                </small>
                            </div>

                        )}
                    />
                    <Controller
                        name={'subject'}
                        control={control}
                        render={({field: {onChange, value}}) => (
                            <div className={styles.inputStyle}>
                                <label htmlFor="subject">Тема письма</label>
                                <InputText id={'subject'} value={value} onChange={(e) => onChange(e.target.value)}/>
                                <small id="subject-help">
                                    Введите сюда заголовок письма
                                </small>
                            </div>
                        )}
                    />
                    <Controller
                        name={'signBody'}
                        control={control}
                        render={({field: {onChange, value}}) => (
                            <div className={styles.signBodyArea}>
                                <span>Подписать сообщение</span>
                                <Checkbox  onChange={e => onChange(e.checked)} checked={value}></Checkbox>
                            </div>
                        )}
                    />
                    <Controller
                        name={'body'}
                        control={control}
                        render={({field: {onChange, value}}) => (
                            <Editor className={styles.editorArea} value={value}
                                    onTextChange={(e) => onChange(e.htmlValue)}/>
                        )}
                    />
                    <Controller
                        name={'signatureCertificate'}
                        control={control}
                        render={({field: {onChange, value}}) => (
                            <FileUpload
                                className={styles.uploader}
                                customUpload={true}
                                onBeforeSelect={()=>{
                                    toast.current?.show({
                                        severity: 'success',
                                        summary: 'Успешно',
                                        detail: 'Не забудьте закрепить ключ',
                                        life: 3000
                                    });
                                }}
                                uploadHandler={(event: FileUploadHandlerEvent)=>{
                                    console.log(event)
                                    onChange(event.files[0])
                                    toast.current?.show({
                                        severity: 'success',
                                        summary: 'Success',
                                        detail: 'Ключ добавлен',
                                        sticky: true
                                    });
                                }}
                                uploadLabel={'Закрепить файлы'}
                                chooseLabel={'Выберите закрытый ключ'}
                                cancelLabel={'Отменить'}
                                emptyTemplate={<p className="m-0">Загрузите сюда ваш сертификат</p>}
                                maxFileSize={1000000}
                            />


                        )}
                    />

                    <Controller
                        name={'attachments'}
                        control={control}
                        render={({field: {onChange, value}}) => (
                    <FileUpload
                        className={styles.uploader}
                        customUpload={true}
                        onBeforeSelect={()=>{
                            toast.current?.show({
                                severity: 'success',
                                summary: 'Успешно',
                                detail: 'Не забудьте закрепить файлы',
                                life: 3000
                            });
                        }}
                        uploadHandler={(event: FileUploadHandlerEvent)=>{
                            onChange(event.files)
                            toast.current?.show({
                                severity: 'success',
                                summary: 'Success',
                                detail: 'Файлы закреплены',
                                sticky: true
                            });
                        }}
                        uploadLabel={'Закрепить файлы'}
                        chooseLabel={'Выбрать файлы'}
                        cancelLabel={'Отменить'}
                        multiple
                        maxFileSize={1000000}
                        emptyTemplate={<p className="m-0">Загрузите сюда ваши файлы</p>}/>
                        )}
                    />
                    <Button label={'Отправить Email'} type={"submit"} className={styles.sendButton}/>
                </form>
            </Dialog>
        </>
    );
};

export default SendEmailDialog;