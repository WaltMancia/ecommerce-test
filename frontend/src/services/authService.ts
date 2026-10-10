import api from "./api";

export const authService = {
  login: async (data: any) => {
    const response = await api.post("/Auth/login", data);
    return response.data;
  },
  register: async (data: any) => {
    const response = await api.post("/Auth/register", data);
    return response.data;
  },
};
