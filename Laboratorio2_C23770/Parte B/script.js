function Agregar() {
  // Seleccionar la lista
  let lista = document.getElementById("miLista");

  // Calcular el número del siguiente elemento
  let num = lista.childElementCount + 1;

  // Verificar si el número es mayor a 10
  if (num > 10) {
    let mensaje = document.createElement("p");
    mensaje.textContent = "Más despacio velocista, para qué más de 10 elementos???";
    mensaje.style.color = "red";
    document.body.appendChild(mensaje);

    // Eliminar el mensaje después de 2 segundos
    setTimeout(() => {
      document.body.removeChild(mensaje);
    }, 2000);
    return; // Salir de la función si hay más de 10 elementos
  }

  // Crear nuevo <li>
  let nuevo = document.createElement("li");
  nuevo.textContent = "Elemento" + num;

  // Agregarlo a la lista
  lista.appendChild(nuevo);
}
function Eliminar() {
  // Seleccionar la lista
  let lista = document.getElementById("miLista");

  // Eliminar el último elemento
  if (lista.childElementCount > 0) {
    lista.removeChild(lista.lastElementChild);
  } else {
   let mensaje = document.createElement("p");
    mensaje.textContent = "No hay elementos para eliminar.";
    mensaje.style.color = "red";
    document.body.appendChild(mensaje);
    
    // Eliminar el mensaje después de 2 segundos
    setTimeout(() => {
      document.body.removeChild(mensaje);
    }, 2000);
  }
}

function Cambiar_fondo() {
  // Cambiar el color de fondo de la lista
  let lista = document.getElementById("miLista");
  lista.style.backgroundColor = lista.style.backgroundColor === "lightblue" ? "" : "lightblue";
}
