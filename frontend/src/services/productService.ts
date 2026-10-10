import api from "./api";

export const productService = {
  getProducts: async () => {
    const response = await api.get("/Products");
    return response.data;
  },
  createProduct: async (productData: any) => {
    const response = await api.post("/Products", productData);
    return response.data;
  },
};
