import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import { ProtectedRoute } from './components/ProtectedRoute';

// Componentes "Placeholder" para probar el enrutamiento y autenticación
const Login = () => <div className="p-8"><h1 className="text-2xl font-bold">Login Page</h1></div>;
const Catalog = () => <div className="p-8"><h1 className="text-2xl font-bold">Catálogo Público</h1></div>;
const Cart = () => <div className="p-8"><h1 className="text-2xl font-bold">Mi Carrito (Protegido)</h1></div>;
const AdminDashboard = () => <div className="p-8"><h1 className="text-2xl font-bold text-red-600">Panel de Administración</h1></div>;
const Unauthorized = () => <div className="p-8"><h1 className="text-2xl font-bold text-red-500">Acceso Denegado</h1></div>;

function App () {
  return (
    <AuthProvider>
      <BrowserRouter>
        <div className="min-h-screen flex flex-col">
          {/* Navegación básica */ }
          <nav className="bg-gray-800 text-white p-4 flex gap-4">
            <a href="/">Catálogo</a>
            <a href="/login">Login</a>
            <a href="/cart">Carrito</a>
            <a href="/admin">Admin</a>
          </nav>

          <main className="flex-grow">
            <Routes>
              {/* Rutas Públicas */ }
              <Route path="/" element={ <Catalog /> } />
              <Route path="/login" element={ <Login /> } />
              <Route path="/unauthorized" element={ <Unauthorized /> } />

              {/* Rutas Protegidas (Customer o Admin) */ }
              <Route element={ <ProtectedRoute /> }>
                <Route path="/cart" element={ <Cart /> } />
              </Route>

              {/* Rutas Protegidas (Solo Admin) */ }
              <Route element={ <ProtectedRoute allowedRoles={ ['Admin'] } /> }>
                <Route path="/admin" element={ <AdminDashboard /> } />
              </Route>

              <Route path="*" element={ <Navigate to="/" replace /> } />
            </Routes>
          </main>
        </div>
      </BrowserRouter>
    </AuthProvider>
  );
}

export default App;