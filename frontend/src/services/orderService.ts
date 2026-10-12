import api from "./api";

export const orderService = {
  checkout: async () => {
    const response = await api.post("/Orders/checkout");
    return response.data;
  },
};
