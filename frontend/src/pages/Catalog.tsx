import { useEffect, useState } from 'react';
import { productService } from '../services/productService';

export const Catalog = () => {
    const [products, setProducts] = useState<any[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetchProducts = async () => {
            try {
                const data = await productService.getProducts();
                setProducts(data);
            } catch (error) {
                console.error("Error cargando productos", error);
            } finally {
                setLoading(false);
            }
        };
        fetchProducts();
    }, []);

    if (loading) return <div className="p-8 text-center">Cargando catálogo...</div>;

    return (
        <div className="p-8 max-w-6xl mx-auto">
            <h1 className="text-3xl font-bold mb-6">Catálogo de Productos</h1>
            { products.length === 0 ? (
                <p className="text-gray-500">No hay productos disponibles por el momento.</p>
            ) : (
                <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                    { products.map((p) => (
                        <div key={ p.id } className="border rounded shadow-sm p-4 bg-white">
                            <h2 className="text-xl font-semibold mb-2">{ p.name }</h2>
                            <p className="text-gray-600 mb-4 h-12 overflow-hidden">{ p.description }</p>
                            <div className="flex justify-between items-center mt-auto">
                                <span className="text-lg font-bold text-green-700">Q{ p.price.toFixed(2) }</span>
                                <button className="bg-gray-800 text-white px-4 py-2 rounded hover:bg-gray-700 text-sm">
                                    Agregar
                                </button>
                            </div>
                        </div>
                    )) }
                </div>
            ) }
        </div>
    );
};