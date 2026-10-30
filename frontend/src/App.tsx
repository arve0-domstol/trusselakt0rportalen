import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { AuthProvider } from './auth/AuthContext'
import { AppLayout } from './components/AppLayout'
import { ProtectedRoute } from './components/ProtectedRoute'
import { AdminUsersPage } from './pages/AdminUsersPage'
import { CaseDetailPage } from './pages/CaseDetailPage'
import { CaseFormPage } from './pages/CaseFormPage'
import { CaseListPage } from './pages/CaseListPage'
import { ForgotPasswordPage } from './pages/ForgotPasswordPage'
import { LoginPage } from './pages/LoginPage'
import { NotificationsPage } from './pages/NotificationsPage'
import { ProfilePage } from './pages/ProfilePage'

function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/glemt-passord" element={<ForgotPasswordPage />} />
          <Route
            element={
              <ProtectedRoute>
                <AppLayout />
              </ProtectedRoute>
            }
          >
            <Route path="/" element={<CaseListPage />} />
            <Route path="/saker/ny" element={<CaseFormPage mode="create" />} />
            <Route path="/saker/:id" element={<CaseDetailPage />} />
            <Route path="/saker/:id/rediger" element={<CaseFormPage mode="edit" />} />
            <Route path="/varsler" element={<NotificationsPage />} />
            <Route path="/min-profil" element={<ProfilePage />} />
            <Route path="/admin/brukere" element={<AdminUsersPage />} />
          </Route>
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  )
}

export default App
