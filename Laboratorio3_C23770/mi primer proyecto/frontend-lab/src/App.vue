<template>
  <ListaPaises :countries="countries" @eliminar="eliminarPais" />
</template>

<script>
import ListaPaises from "./components/CountriesList.vue";

export default {
  components: {
    ListaPaises,
  },
  data() {
    return {
      countries: [
        { name: "Costa Rica", continent: "América", language: "Español" },
        { name: "Japón", continent: "Asia", language: "Japonés" },
        { name: "Corea del Sur", continent: "Asia", language: "Coreano" },
        { name: "Italia", continent: "Europa", language: "Italiano" },
        { name: "Alemania", continent: "Europa", language: "Alemán" },
      ],
    };
  },
  methods: {
    eliminarPais(nombre) {
      this.countries = this.countries.filter((country) => country.name !== nombre);
    },
  },
};
</script>

<style>

</style>
