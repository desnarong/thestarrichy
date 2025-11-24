// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

async function getJwtToken() {
    try {
        const response = await fetch('/home/GetToken', {
            method: 'GET',
            credentials: 'include' // ส่ง Cookie ไปด้วย
        });
        const data = await response.json();
        if (data.token) {
            //console.log('JWT Token:', data.token);
            return data.token;
        } else {
            //console.error('No token found');
            return null;
        }
    } catch (error) {
        console.error('Error fetching token:', error);
        return null;
    }
}