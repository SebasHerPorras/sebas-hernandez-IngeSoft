import { createApp } from 'vue'
import App from './App.vue'
import {createRouter, createWebHistory} from 'vue-router'
import CountriesList from './components/CountriesList.vue'
import CountryFrom from './components/CountryFrom.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {path: '/', name : "Home", component: CountriesList},
    {path: '/country', name: "CountryForm", component: CountryFrom},
  ],
});

createApp(App).use(router).mount('#app')
