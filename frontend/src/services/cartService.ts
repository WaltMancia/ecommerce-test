import api from './api';

export const cartService = {
    getCart: async () => {
        const response = await api.get('/Cart');
        return response.data;
    },
    addItem: async (productId: number, quantity: number = 1) => {
        const response = await api.post('/Cart/items', { productId, quantity });
        return response.data;
    },
    removeItem: async (productId: number) => {
        const response = await api.delete(`/Cart/items/${productId}`);
        return response.data;
    }
};