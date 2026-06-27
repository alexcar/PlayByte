/**
 * Configuração de ambiente. `apiBaseUrl` aponta para a API C# PlayByte.
 *
 * Em desenvolvimento/homologação a API roda em http://localhost:5080 (perfil "http"
 * de launchSettings.json). Usamos HTTP — e não HTTPS — para evitar atrito com o
 * certificado de desenvolvimento auto-assinado ao chamar a API a partir do Angular.
 */
export const environment = {
  production: false,
  apiBaseUrl: 'http://localhost:5080/api',
};
