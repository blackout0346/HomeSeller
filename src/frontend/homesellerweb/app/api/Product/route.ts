export  async function GetProducts() {

const response = await fetch("http://localhost:5030/api/Product/GetProducts")

return response.json()
}