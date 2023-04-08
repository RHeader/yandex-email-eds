export const requestWithAuth = async (url: string, options: RequestInit = {}): Promise<Response> => {
    const cookie = document.cookie;

    const headers = { ...options.headers, 'Cookie': cookie };

    if(import.meta.env.PROD){
        const response = await fetch(url, {
            ...options, headers });

        return response;
    }
    const response = await fetch(url, {
        credentials: 'include',
        ...options, headers });

    return response;
};