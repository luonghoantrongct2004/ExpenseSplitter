"use client"
import { QueryClient, QueryClientProvider } from "@tanstack/react-query"
import { GoogleOAuthProvider } from '@react-oauth/google';
import React, { useState } from 'react';
import { Toaster } from 'react-hot-toast';

export function Providers({children}: {children: React.ReactNode}){
    const[queryClient] = useState(()=> new QueryClient());

    return (
        <GoogleOAuthProvider clientId={process.env.NEXT_PUBLIC_GOOGLE_CLIENT_ID!}>
            <QueryClientProvider client={queryClient}>
                {children}
                <Toaster position="top-right" />
            </QueryClientProvider>
        </GoogleOAuthProvider>
    )
}