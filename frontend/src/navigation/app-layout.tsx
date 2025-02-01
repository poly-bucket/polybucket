import { Outlet, Link, useNavigate } from "react-router-dom";
import { routes } from "./routes";
import { useAppDispatch, useAppSelector } from '../store/hooks';
import { logout } from '../store/slices/auth-slice';

interface AppLayoutProps {
  children?: React.ReactNode;
}

export const AppLayout: React.FC<AppLayoutProps> = ({ children }) => {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { isAuthenticated, user } = useAppSelector((state) => state.auth);

  const handleLogout = () => {
    dispatch(logout());
    navigate(routes.login);
  };

  return (
    <div className="flex flex-col min-h-screen">
      <nav className="sticky top-0 z-10 bg-black border-b border-green-900">
        <div className="max-w-[2000px] mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex items-center justify-between h-16">
            <div className="flex items-center">
              <a href={routes.dashboard} className="flex items-center">
                <span className="text-xl font-semibold text-green-400">PolyBucket</span>
              </a>
            </div>
            <div className="flex-1 max-w-2xl mx-4">
              <div className="relative">
                <div className="absolute inset-y-0 left-0 flex items-center pl-3 pointer-events-none">
                  <svg className="w-5 h-5 text-green-400" fill="currentColor" viewBox="0 0 20 20" xmlns="http://www.w3.org/2000/svg">
                    <path fillRule="evenodd" d="M8 4a4 4 0 100 8 4 4 0 000-8zM2 8a6 6 0 1110.89 3.476l4.817 4.817a1 1 0 01-1.414 1.414l-4.816-4.816A6 6 0 012 8z" clipRule="evenodd"></path>
                  </svg>
                </div>
                <input
                  type="text"
                  className="block w-full py-2 pl-10 pr-3 ml-6 bg-green-400 border rounded-lg border-green-500/30 focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-transparent"
                  placeholder="Search models..."
                />
              </div>
            </div>
            <div className="flex items-center space-x-4">
              <Link to={routes.dashboard} className="px-3 py-2 text-sm font-medium text-green-400 rounded-md hover:text-blue-600">
                Home
              </Link>
              {isAuthenticated ? (
                <>
                  <Link to={routes.profile} className="px-3 py-2 text-sm font-medium text-green-400 rounded-md hover:text-blue-600">
                    Profile
                  </Link>
                  {user?.role === 'Admin' && (
                    <>
                      <Link to={routes.plugins} className="px-3 py-2 text-sm font-medium text-green-400 rounded-md hover:text-blue-600">
                        Plugins
                      </Link>
                      <Link to={routes.reports} className="px-3 py-2 text-sm font-medium text-green-400 rounded-md hover:text-blue-600">
                        Reports
                      </Link>
                    </>
                  )}
                  <Link to={routes.settings} className="px-3 py-2 text-sm font-medium text-green-400 rounded-md hover:text-blue-600">
                    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                    </svg>
                  </Link>
                  <button
                    onClick={handleLogout}
                    className="px-3 py-2 text-sm font-medium text-green-400 rounded-md hover:text-red-500"
                  >
                    Logout
                  </button>
                </>
              ) : (
                <Link to={routes.login} className="px-3 py-2 text-sm font-medium text-green-400 rounded-md hover:text-blue-600">
                  Login
                </Link>
              )}
            </div>
          </div>
        </div>
      </nav>
      <main className="flex-1 w-full bg-black">
        <div className="max-w-[2000px] mx-auto px-4 sm:px-6 lg:px-8 py-6">
          {children || <Outlet />}
        </div>
      </main>
    </div>
  );
};