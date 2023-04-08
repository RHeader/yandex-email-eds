import React, {FC} from 'react';

import styles from './DownloadableIcon.module.scss'
import {PrimeIcons} from "primereact/api";

interface DownloadIconProps {
    base64: string;
    filename: string;
    contentType: string;
}

const DownloadableIcon:FC<DownloadIconProps> = ({base64,filename,contentType}) => {

    const downloadFile = () => {
        if(base64.length > 0) {
            const byteCharacters = atob(base64);
            const byteNumbers = new Array(byteCharacters.length);
            for (let i = 0; i < byteCharacters.length; i++) {
                byteNumbers[i] = byteCharacters.charCodeAt(i);
            }
            const byteArray = new Uint8Array(byteNumbers);
            const blob = new Blob([byteArray], {type: contentType});
            const url = URL.createObjectURL(blob);
            const link = document.createElement("a");
            link.href = url;
            link.download = filename;
            link.click();
            URL.revokeObjectURL(url);
        }
    };

    return (
        <div onClick={()=>downloadFile()} className={styles.attachmentBox}>
            <i className={PrimeIcons.FILE} />
            {filename}
        </div>
    );
};

export default DownloadableIcon;