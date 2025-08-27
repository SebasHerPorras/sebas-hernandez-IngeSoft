<template>
  <div class="container mt-5">
    <h1 class="display-4 text-center">Lista de países</h1>
    <table
      class="table is-bordered is-striped is-narrow is-hoverable is-fullwidth"
    >
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
            <button class="btn btn-danger btn-sm" @click="$emit('eliminar', country.name)">Eliminar</button>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script>
export default {
  name: "CountriesList",
  props: {
    countries: {
      type: Array,
      required: true,
    },
  },
};
</script>

<style lang="scss" scoped></style>
