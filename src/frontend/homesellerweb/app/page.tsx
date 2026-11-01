
import { Suspense } from "react";
import {} from "./layout";
import {  Card,

  CardTitle,
  CardAction,
  CardDescription,

  CardImage, } from "../components/ui/card";
import {InputInline} from "../components/ui/input";
import {Footer} from "../components/ui/footer";


  export default async function Home() {

  const product={name:"Ремонт холодильников, ларей, торг оборудования",image:"https://images.genius.com/72679941f2dab3deeb3846390eb49655.1000x1000x1.jpg",
     description:"this is a nice product", price:300}
  
  

 
  return (
   
      <Suspense fallback={<div>Loading...</div>}>
        <InputInline placeholder="Search for products..." />
       <div className="grid grid-cols-3 " >


     <Card size="sm" className="w-80">
     <CardImage src={product.image}> {product.image}</CardImage>
     
      <CardTitle>{product.name}</CardTitle>
      <CardDescription>{product.description}</CardDescription>
       <CardAction>Город Улан-Удэ Ул.улица</CardAction>
           
   </Card>
     
    <Card size="sm" className="w-80">
     <CardImage src={product.image}> {product.image}</CardImage>
     
      <CardTitle>{product.name}</CardTitle>
      <CardDescription>{product.description}</CardDescription>
      <CardAction>Город Улан-Удэ Ул.улица</CardAction>
           
   </Card>
     <Card size="sm" className="w-80">
     <CardImage src={product.image}> {product.image}</CardImage>
     
      <CardTitle>{product.name}</CardTitle>
      <CardDescription>{product.description}</CardDescription>
      <CardAction>Город Улан-Удэ Ул.улица</CardAction>
           
   </Card>
  
      </div>
     
        <Footer />
      </Suspense>
  

  );
}
