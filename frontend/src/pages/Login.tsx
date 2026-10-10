import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { authService } from '../services/authService';

export const Login = () => {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const { login } = useAuth();
    const navigate = useNavigate();

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError('');
        try {
            const data = await authService.login({ email, password });
            login(data.token); // Guardamos el token en contexto y localStorage
            navigate('/'); // Redirigimos al inicio
        } catch (err: any) {
            // Mitigación: Mostramos el error genérico que manda la API (ej. "Credenciales inválidas")
            setError(err.response?.data?.error || 'Error al iniciar sesión');
        }
    };

    return (
        <div className="flex justify-center items-center h-[80vh]">
            <form onSubmit={ handleSubmit } className="bg-white p-8 rounded shadow-md w-96 border border-gray-200">
                <h2 className="text-2xl font-bold mb-6 text-center">Iniciar Sesión</h2>
                { error && <p className="text-red-500 text-sm mb-4">{ error }</p> }

                <div className="mb-4">
                    <label className="block text-sm font-medium mb-1">Email</label>
                    <input
                        type="email"
                        value={ email }
                        onChange={ (e) => setEmail(e.target.value) }
                        className="w-full border rounded p-2"
                        required
                    />
                </div>

                <div className="mb-6">
                    <label className="block text-sm font-medium mb-1">Contraseña</label>
                    <input
                        type="password"
                        value={ password }
                        onChange={ (e) => setPassword(e.target.value) }
                        className="w-full border rounded p-2"
                        required
                    />
                </div>

                <button type="submit" className="w-full bg-blue-600 text-white p-2 rounded hover:bg-blue-700 transition">
                    Ingresar
                </button>
            </form>
        </div>
    );
};