/**
 * API Configuration
 * Centraliza todas las URLs y endpoints de la API
 */

// Base URL de la API
const API_BASE_URL =
  process.env.VUE_APP_API_BASE_URL ||
  "https://localhost:7183/api/CoffeeMachine";

/**
 * Construye la URL completa de un endpoint
 * @param {string} action - Nombre de la acción del controlador
 * @returns {string} URL completa
 */
const buildApiUrl = (action) => {
  return `${API_BASE_URL}/${action}`;
};

/**
 * Endpoints específicos de la Coffee Machine API
 */
export const API_ENDPOINTS = {
  // Obtener inventario de cafés
  GET_INVENTORY: buildApiUrl("inventory"),

  // Obtener precios de cafés
  GET_PRICES: buildApiUrl("prices"),

  // Obtener cambio disponible
  GET_CHANGE: buildApiUrl("change"),

  // Realizar orden de café
  PLACE_ORDER: buildApiUrl("orders"),
};

/**
 * Configuración de headers por defecto
 */
export const DEFAULT_HEADERS = {
  "Content-Type": "application/json",
  Accept: "application/json",
};

/**
 * Timeout para requests (en milisegundos)
 */
export const REQUEST_TIMEOUT = 10000;

export default {
  API_BASE_URL,
  API_ENDPOINTS,
  DEFAULT_HEADERS,
  REQUEST_TIMEOUT,
};
