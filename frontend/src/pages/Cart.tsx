import { useEffect, useState } from 'react';
import { cartService } from '../services/cartService';
import { orderService } from '../services/orderService';

export const Cart = () => {
    const [cart, setCart] = useState<any>(null);
    const [message, setMessage] = useState('');
    const [loading, setLoading] = useState(true);

    const loadCart = async () => {
        try {
            const data = await cartService.getCart();
            setCart(data);
        } catch (error) {
            console.error("Error cargando carrito", error);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => { loadCart(); }, []);

    const handleRemove = async (productId: number) => {
        await cartService.removeItem(productId);
        loadCart();
    };

    const handleCheckout = async () => {
        try {
            setMessage('Procesando pago...');
            const result = await orderService.checkout();
            setMessage(`¡Compra exitosa! Ref: ${result.transactionId}`);
            setCart(null); // Limpiamos la vista
        } catch (error: any) {
            setMessage(error.response?.data?.error || 'Error al procesar el pago.');
        }
    };

    if (loading) return <div className="p-8 text-center">Cargando...</div>;

    return (
        <div className="p-8 max-w-4xl mx-auto">
            <h1 className="text-3xl font-bold mb-6">Tu Carrito</h1>

            { message && (
                <div className={ `p-4 mb-6 rounded ${message.includes('exito') ? 'bg-green-100 text-green-800' : 'bg-blue-100 text-blue-800'}` }>
                    { message }
                </div>
            ) }

            { !cart || !cart.items || cart.items.length === 0 ? (
                <p>Tu carrito está vacío.</p>
            ) : (
                <div className="bg-white p-6 rounded shadow border">
                    { cart.items.map((item: any) => (
                        <div key={ item.productId } className="flex justify-between items-center border-b py-4">
                            <div>
                                <h3 className="font-bold">{ item.productName }</h3>
                                <p className="text-gray-600">Cant: { item.quantity } x Q{ item.unitPrice.toFixed(2) }</p>
                            </div>
                            <div className="flex items-center gap-4">
                                <span className="font-bold">Q{ item.subtotal.toFixed(2) }</span>
                                <button onClick={ () => handleRemove(item.productId) } className="text-red-500 hover:text-red-700 text-sm">
                                    Eliminar
                                </button>
                            </div>
                        </div>
                    )) }
                    <div className="mt-6 flex justify-between items-center text-xl font-bold">
                        <span>Total:</span>
                        <span>Q{ cart.totalAmount.toFixed(2) }</span>
                    </div>
                    <button onClick={ handleCheckout } className="mt-6 w-full bg-green-600 text-white p-3 rounded hover:bg-green-700 font-bold">
                        Pagar Ahora
                    </button>
                </div>
            ) }
        </div>
    );
};