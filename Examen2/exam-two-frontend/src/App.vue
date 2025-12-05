<template>
  <div id="app">
    <CoffeeMachine />
  </div>
</template>

<script>
import CoffeeMachine from './components/CoffeeMachine.vue'

export default {
  name: 'App',
  components: {
    CoffeeMachine
  }
}
</script>

<style>
* {
  box-sizing: border-box;
  margin: 0;
  padding: 0;
}

body {
  background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
  min-height: 100vh;
}

#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  padding: 20px;
}
</style>
