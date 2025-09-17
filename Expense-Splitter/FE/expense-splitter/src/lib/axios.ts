import axios from "axios";
import { getCookie } from 'cookies-next';

const instance = axios.create({
    baseURL: process.env.NEXT_PUBLIC_API_URL,
    timeout: 10000,
})
instance.interceptors.request.use(
    (config) => {
        const token = getCookie('access_token');
        if(token){
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => {
        return Promise.reject(error);
    }
)
instance.interceptors.response.use(
    (response) => response,
    async (error) => {
        if(error.response?.statusCode === 401){
            window.location.href = '/auth/login';
        }
        return Promise.reject(error);
    }
)

export default instance;