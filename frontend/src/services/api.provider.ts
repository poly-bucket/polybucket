import { ApiClientFactory } from '../api/clientFactory';
import { LoginCommand, LoginCommandResponse } from '../api/client';

export const loginClient = {
  login: (command: LoginCommand): Promise<LoginCommandResponse> =>
    ApiClientFactory.getApiClient().login_Login(command)
};
