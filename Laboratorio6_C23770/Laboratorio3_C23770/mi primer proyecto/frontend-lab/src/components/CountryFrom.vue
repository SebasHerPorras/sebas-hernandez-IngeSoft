<template>
  <div class="d-flex justify-content-center align-items-center vh-100">
    <div class="card p-4 shadow" style="max-width: 400px; width: 100%">
      <h3 class="text-center">Formulario de creación de países</h3>
      <form @submit.prevent="saveCountry">
        <div class="form-group">
          <label for="nombre">Nombre:</label>
          <input
            v-model="formData.Name"
            type="text"
            id="name"
            class="form-control"
            required
          />
        </div>
        <div class="form-group">
          <label for="continente">Continente:</label>
          <select
            v-model="formData.Continent"
            id="continente"
            required
            class="form-control"
          >
            <option value="" disabled>Seleccione un continente</option>
            <option>África</option>
            <option>Asia</option>
            <option>Europa</option>
            <option>América</option>
            <option>Oceanía</option>
            <option>Antártida</option>
          </select>
        </div>
        <div class="form-group">
          <label for="idioma">Idioma:</label>
          <input
            v-model="formData.Language"
            type="text"
            id="idioma"
            class="form-control"
            required
          />
        </div>
        <div>
          <button type="submit" class="btn btn-success w-100">Guardar</button>
        </div>
      </form>
    </div>
  </div>
</template>

<script>
import axios from"axios";
export default {
  name: "CountryForm",
  data() {
    return {
      formData: { Name: "", Continent: "", Language: "" },
    };
  },
  methods: {
    saveCountry() {
      console.log("Datos a guardar:", this.formData);
      axios
        .post("http://localhost:5166/api/Country", {
          Name: this.formData.Name,
          Continent: this.formData.Continent,
          Language: this.formData.Language,
        })
        .then(function (response) {
          console.log(response);
          window.location.href = "/";
        })
        .catch(function (error) {
          console.log(error);
        });
    },
  },
};
</script>
