<template>
  <div class="container mt-5">
    <h1 class="display-4 text-center">Lista de países</h1>

    <!-- Botón Agregar país fuera de la tabla -->
    <div class="d-flex justify-content-end mb-3">
      <a href="/country">
        <button type="button" class="btn btn-outline-secondary">
          Agregar país
        </button>
      </a>
    </div>

    <!-- Tabla de países -->
    <table class="table is-bordered is-striped is-narrow is-hoverable is-fullwidth">
      <thead>
        <tr>
          <th>Nombre</th>
          <th>Continente</th>
          <th>Idioma</th>
          <th>Acciones</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="country in countries" :key="country.name">
          <td>{{ country.name }}</td>
          <td>{{ country.continent }}</td>
          <td>{{ country.language }}</td>
          <td>
            <button class="btn btn-secondary btn-sm">Editar</button>
            <button
              class="btn btn-danger btn-sm"
              @click="eliminarPais(country.name)"
            >
              Eliminar
            </button>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script>
import axios from "axios";

export default {
  name: "CountriesList",
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
      this.countries = this.countries.filter(
        (country) => country.name !== nombre
      );
    },
    getCountries() {
      axios
        .get("http://localhost:5166/api/Country")
        .then((response) => {
          this.countries = response.data;
        })
        .catch((err) => console.log(err));
    },
  },
  created() {
    this.getCountries();
  },
};
</script>

<style lang="scss" scoped></style>
