import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

interface Props {
    allowedRoles?: string[];
}

export const ProtectedRoute = ({ allowedRoles }: Props) => {
    const { user, isAuthenticated } = useAuth();

    if (!isAuthenticated) {
        return <Navigate to="/login" replace />;
    }

    if (allowedRoles && user && !allowedRoles.includes(user.role)) {
        // Un Customer intentando entrar al panel de Admin
        return <Navigate to="/unauthorized" replace />;
    }

    return <Outlet />;
};