import React, {FC, useRef, useState} from 'react';
import {Dialog} from "primereact/dialog";
import {FileUpload, FileUploadHandlerEvent} from "primereact/fileupload";
import {host} from "../../../envt";
import {Toast} from "primereact/toast";

interface ISendVerifyEdsDialog {
    visible: boolean;
    onHide: (visible: boolean) => void;
}


function getExtension(filename: string) {
    return filename.split('.').pop()
}

const SendVerifyEds: FC<ISendVerifyEdsDialog> = ({visible, onHide}) => {

    const toast = useRef<Toast>(null);


    const verifyEds = async (event: FileUploadHandlerEvent) => {
        const form = new FormData();
        const crt = event.files.find(x => getExtension(x.name) == 'crt')
        if (!crt) return
        form.append('publicCertificate', crt)
        const signature = event.files.find(x => getExtension(x.name) == 'sig')
        if (!signature) return
        form.append('signature', signature)
        const otherFile = event.files.find(x => getExtension(x.name) !== 'sig' || getExtension(x.name) !== 'crt')
        if (!otherFile) return
        form.append('file', otherFile)

        const response = await fetch(`${host}/email/verify`,{
            method: 'POST',
            credentials: 'include',
            body:form
        })

        if(response.ok){
            const body = await response.json()

            if(body.verify){
                    toast.current?.show({severity:'success', summary: 'Success', detail:'Верификация ЭЦП успешна', sticky:true});
            }else{
                toast.current?.show({severity:'error', summary: 'Error', detail:'Верификация ЭЦП не успешна', sticky:true});
            }
            onHide(false)
            return
        }
        alert('Ошибка верификации подписи')
    }

    return (
        <>
        <Toast ref={toast} />
    <Dialog header={'Верификация сообщения с ЭЦП'} visible={visible}
                onHide={() => onHide(false)}>
            <FileUpload
                customUpload={true}
                uploadHandler={verifyEds} multiple
                maxFileSize={1000000}
                emptyTemplate={<p className="m-0">Загрузите сюда публичный сертифакт формата certificate.crt,
                    подпись формата name.sig и файл для верификации</p>}/>
        </Dialog>
        </>
    );
};

export default SendVerifyEds;