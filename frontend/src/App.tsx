import { RouterProvider } from "react-router-dom";
import { Provider } from 'react-redux';
import { router } from "./navigation/router";
import { store } from './store/store';

const App: React.FC = () => {
  return (
    <Provider store={store}>
      <RouterProvider router={router} />
    </Provider>
  );
};

export default App;