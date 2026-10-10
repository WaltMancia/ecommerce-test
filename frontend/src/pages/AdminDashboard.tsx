import React, { useState } from 'react';
import { productService } from '../services/productService';
import { useAuth } from '../context/AuthContext';

export const AdminDashboard = () => {
    const { user } = useAuth();
    const [formData, setFormData] = useState({
        name: '', description: '', price: 0, stock: 0, categoryId: 1
    });
    const [message, setMessage] = useState('');

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        try {
            await productService.createProduct(formData);
            setMessage('Producto creado exitosamente');
            setFormData({ name: '', description: '', price: 0, stock: 0, categoryId: 1 });
        } catch (error: any) {
            setMessage(error.response?.data?.error || 'Error al crear producto. ¿Tienes permisos?');
        }
    };

    return (
        <div className="p-8 max-w-4xl mx-auto">
            <h1 className="text-3xl font-bold mb-6 text-red-700">Panel de Administración</h1>
            <p className="mb-6">Bienvenido administrador: <strong>{ user?.email }</strong></p>

            <div className="bg-white p-6 rounded shadow border">
                <h2 className="text-xl font-bold mb-4">Agregar Nuevo Producto</h2>
                { message && <div className="mb-4 p-2 bg-blue-100 text-blue-800 rounded">{ message }</div> }

                <form onSubmit={ handleSubmit } className="grid grid-cols-2 gap-4">
                    <div className="col-span-2">
                        <label className="block text-sm mb-1">Nombre</label>
                        <input type="text" className="w-full border p-2 rounded" required
                            value={ formData.name } onChange={ e => setFormData({ ...formData, name: e.target.value }) } />
                    </div>
                    <div className="col-span-2">
                        <label className="block text-sm mb-1">Descripción</label>
                        <textarea className="w-full border p-2 rounded" required
                            value={ formData.description } onChange={ e => setFormData({ ...formData, description: e.target.value }) } />
                    </div>
                    <div>
                        <label className="block text-sm mb-1">Precio</label>
                        <input type="number" step="0.01" className="w-full border p-2 rounded" required
                            value={ formData.price } onChange={ e => setFormData({ ...formData, price: parseFloat(e.target.value) }) } />
                    </div>
                    <div>
                        <label className="block text-sm mb-1">Stock</label>
                        <input type="number" className="w-full border p-2 rounded" required
                            value={ formData.stock } onChange={ e => setFormData({ ...formData, stock: parseInt(e.target.value) }) } />
                    </div>
                    <button type="submit" className="col-span-2 bg-red-700 text-white p-2 rounded hover:bg-red-800">
                        Guardar Producto
                    </button>
                </form>
            </div>
        </div>
    );
};