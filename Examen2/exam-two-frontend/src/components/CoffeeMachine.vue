<template>
  <div>
    <h1>Máquina de Café</h1>
    <div v-if="loading">Cargando...</div>
    <div v-if="error" style="color:red">{{ error }}</div>

    <div v-if="!loading && inventory">
      <h2>Inventario</h2>
      <ul>
        <li v-for="(qty, name) in inventory" :key="name">
          {{ name }}: {{ qty }} disponibles (₡{{ prices[name] }})
        </li>
      </ul>

      <h2>Pedido</h2>
      <div v-if="hasAvailableStock">
        <div v-for="(qty, name) in inventory" :key="name" v-show="qty > 0">
          <label>{{ name }}:</label>
          <input type="number" v-model.number="order[name]" min="0" :max="qty" />
        </div>
        <div>
          <label>Monto de pago (₡):</label>
          <input 
            type="text" 
            v-model="payment"
            @input="handlePaymentInput"
            @blur="handlePaymentBlur"
            pattern="[0-9]*"
            inputmode="numeric"
            placeholder="0"
            class="payment-input"
          />
        </div>
        <button @click="placeOrder">Ordenar</button>
      </div>
      <div v-else class="no-stock-message">
        <p>Lo sentimos, la tienda se quedó sin stock de café, esperamos poder volver a verlo pronto :)</p>
      </div>
    </div>

    <div v-if="result">
      <h3>{{ result.success ? 'Pedido exitoso' : 'Error' }}</h3>
      <p>{{ result.message }}</p>
      <div v-if="result.changeBreakdown">
        <h4>Cambio:</h4>
        <ul>
          <li v-for="(qty, coin) in result.changeBreakdown" :key="coin">
            {{ qty }} moneda(s) de ₡{{ coin }}
          </li>
        </ul>
      </div>
    </div>
  </div>
</template>

<script>
import { API_ENDPOINTS, DEFAULT_HEADERS, REQUEST_TIMEOUT } from '@/config/apiConfig';

export default {
  name: 'CoffeeMachine',
  data() {
    return {
      loading: true,
      error: null,
      inventory: null,
      prices: {},
      order: {},
      payment: '0',
      result: null,
    };
  },
  computed: {
    hasAvailableStock() {
      if (!this.inventory) return false;
      return Object.values(this.inventory).some(qty => qty > 0);
    }
  },
  async mounted() {
    await this.loadData();
  },
  methods: {
    async loadData() {
      this.loading = true;
      this.error = null;
      console.log('🔄 Iniciando carga de datos...');
      console.log('📍 GET_INVENTORY URL:', API_ENDPOINTS.GET_INVENTORY);
      console.log('📍 GET_PRICES URL:', API_ENDPOINTS.GET_PRICES);
      try {
        const [invRes, priceRes] = await Promise.all([
          fetch(API_ENDPOINTS.GET_INVENTORY, {
            headers: DEFAULT_HEADERS,
            signal: AbortSignal.timeout(REQUEST_TIMEOUT)
          }).then(r => {
            console.log('📦 Respuesta inventario status:', r.status);
            return r.json();
          }),
          fetch(API_ENDPOINTS.GET_PRICES, {
            headers: DEFAULT_HEADERS,
            signal: AbortSignal.timeout(REQUEST_TIMEOUT)
          }).then(r => {
            console.log('💰 Respuesta precios status:', r.status);
            return r.json();
          }),
        ]);
        console.log('📦 Inventario response:', invRes);
        console.log('💰 Precios response:', priceRes);
        
        if (invRes.coffeeInventory && priceRes.coffeePrices) {
          console.log('✅ Datos cargados correctamente');
          this.inventory = invRes.coffeeInventory;
          this.prices = priceRes.coffeePrices;
          this.order = Object.keys(this.inventory).reduce((acc, k) => { acc[k] = 0; return acc; }, {});
        } else {
          console.error('❌ Formato de respuesta incorrecto');
          console.error('invRes:', invRes);
          console.error('priceRes:', priceRes);
          this.error = 'No se pudo cargar inventario o precios';
        }
      } catch (e) {
        console.error('❌ Error al cargar datos:', e);
        console.error('Error completo:', e.message, e.stack);
        this.error = 'Error de conexión con el backend: ' + e.message;
      } finally {
        this.loading = false;
      }
    },
    handlePaymentInput(event) {
      const value = event.target.value.replace(/[^0-9]/g, '');
      this.payment = value || '';
    },
    handlePaymentBlur() {
      if (this.payment === '' || this.payment === null || this.payment === undefined) {
        this.payment = '0';
      }
    },
    async placeOrder() {
      console.log('🛒 Iniciando orden...');
      
      // Validar que hay al menos un café seleccionado
      const orderItems = Object.fromEntries(Object.entries(this.order).filter(([, qty]) => qty > 0));
      if (Object.keys(orderItems).length === 0) {
        this.result = { 
          success: false, 
          message: '⚠️ Debe seleccionar al menos un café para ordenar' 
        };
        return;
      }

      // Validar que el pago sea mayor a 0 si hay cafés seleccionados
      const paymentAmount = this.payment === '' || this.payment === null ? 0 : parseInt(this.payment);
      if (paymentAmount <= 0) {
        this.result = { 
          success: false, 
          message: '⚠️ El monto de pago debe ser mayor a 0' 
        };
        return;
      }

      const orderData = {
        order: orderItems,
        payment: {
          totalAmount: paymentAmount,
          coins: [],
          bills: []
        }
      };
      console.log('📝 Datos de la orden:', orderData);
      console.log('📍 PLACE_ORDER URL:', API_ENDPOINTS.PLACE_ORDER);
      try {
        const res = await fetch(API_ENDPOINTS.PLACE_ORDER, {
          method: 'POST',
          headers: DEFAULT_HEADERS,
          body: JSON.stringify(orderData),
          signal: AbortSignal.timeout(REQUEST_TIMEOUT)
        });
        console.log('📮 Respuesta orden status:', res.status);
        const data = await res.json();
        console.log('📮 Respuesta orden data:', data);
        this.result = data;
        await this.loadData();
      } catch (e) {
        console.error('❌ Error al realizar pedido:', e);
        this.result = { success: false, message: 'Error al realizar el pedido: ' + e.message };
      }
    }
  }
};
</script>

<style scoped>
* {
  box-sizing: border-box;
}

div {
  max-width: 800px;
  margin: 0 auto;
  padding: 20px;
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
}

h1 {
  text-align: center;
  color: #4a2c2a;
  font-size: 2.5rem;
  margin-bottom: 30px;
  text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.1);
}

h1::before {
  content: "☕ ";
}

h2 {
  color: #6f4e37;
  border-bottom: 3px solid #d4a574;
  padding-bottom: 10px;
  margin-top: 30px;
  margin-bottom: 20px;
  font-size: 1.8rem;
}

h3 {
  color: #8b6f47;
  margin-top: 25px;
  font-size: 1.5rem;
}

h4 {
  color: #a0826d;
  margin-top: 15px;
  font-size: 1.2rem;
}

/* Loading y error */
div[style*="color:red"],
.error-message {
  background: #ffe6e6;
  border: 2px solid #ff4444;
  border-radius: 8px;
  padding: 15px;
  color: #cc0000 !important;
  margin: 20px 0;
  font-weight: 600;
}

div:has(> *:first-child:only-child) {
  text-align: center;
  padding: 40px;
  font-size: 1.2rem;
  color: #666;
}

/* Listas */
ul {
  list-style: none;
  padding: 0;
  margin: 20px 0;
}

li {
  background: linear-gradient(135deg, #fff9f0 0%, #ffe8d6 100%);
  border: 2px solid #d4a574;
  border-radius: 10px;
  padding: 15px 20px;
  margin-bottom: 12px;
  font-size: 1.1rem;
  color: #4a2c2a;
  transition: all 0.3s ease;
  box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
}

li:hover {
  transform: translateX(5px);
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.15);
  background: linear-gradient(135deg, #fff5e6 0%, #ffd9b3 100%);
}

/* Formulario */
label {
  display: inline-block;
  margin-right: 15px;
  font-weight: 600;
  color: #4a2c2a;
  min-width: 150px;
  font-size: 1rem;
}

input[type="number"],
input[type="text"] {
  width: 120px;
  padding: 8px 12px;
  margin-bottom: 12px;
  border: 2px solid #d4a574;
  border-radius: 6px;
  font-size: 1rem;
  transition: all 0.3s ease;
  background: white;
}

input[type="number"]:focus,
input[type="text"]:focus {
  outline: none;
  border-color: #6f4e37;
  box-shadow: 0 0 0 3px rgba(111, 78, 55, 0.1);
  transform: scale(1.05);
}

input[type="number"]:hover,
input[type="text"]:hover {
  border-color: #8b6f47;
}

/* Botón */
button {
  margin-top: 20px;
  padding: 15px 40px;
  background: linear-gradient(135deg, #6f4e37 0%, #4a2c2a 100%);
  color: white;
  border: none;
  border-radius: 8px;
  font-size: 1.1rem;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.2);
  text-transform: uppercase;
  letter-spacing: 1px;
  display: block;
  width: 100%;
  max-width: 300px;
  margin-left: auto;
  margin-right: auto;
}

button:hover {
  background: linear-gradient(135deg, #8b6f47 0%, #6f4e37 100%);
  transform: translateY(-2px);
  box-shadow: 0 6px 15px rgba(0, 0, 0, 0.3);
}

button:active {
  transform: translateY(0);
  box-shadow: 0 2px 5px rgba(0, 0, 0, 0.2);
}

/* Resultado */
div:has(> h3) {
  background: linear-gradient(135deg, #f0f9ff 0%, #e0f2fe 100%);
  border: 3px solid #0284c7;
  border-radius: 12px;
  padding: 25px;
  margin-top: 30px;
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.1);
}

div:has(> h3:contains("Error")) {
  background: linear-gradient(135deg, #ffe6e6 0%, #ffcccc 100%);
  border-color: #dc2626;
}

div:has(> h3:contains("exitoso")) {
  background: linear-gradient(135deg, #f0fdf4 0%, #dcfce7 100%);
  border-color: #16a34a;
}

p {
  font-size: 1.1rem;
  line-height: 1.6;
  color: #374151;
  margin: 15px 0;
}

/* Sección de pedido */
div:has(> h2:contains("Pedido")) > div {
  background: white;
  padding: 12px;
  border-radius: 8px;
  margin-bottom: 8px;
  border-left: 4px solid #d4a574;
}

/* Mensaje sin stock */
.no-stock-message {
  background: linear-gradient(135deg, #fff3cd 0%, #ffe69c 100%);
  border: 3px solid #ff9800;
  border-radius: 12px;
  padding: 30px;
  margin-top: 20px;
  text-align: center;
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.1);
}

.no-stock-message p {
  font-size: 1.4rem;
  font-weight: 700;
  color: #b35600;
  margin: 0;
}

/* Animaciones */
@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

div {
  animation: fadeIn 0.5s ease;
}
</style>