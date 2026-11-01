
import { Suspense } from "react";
import {} from "./layout";
import {  Card,
  CardHeader,
  CardFooter,
  CardTitle,
  CardAction,
  CardDescription,
  CardContent,
  CardImage, } from "../components/ui/card";
import {InputInline} from "../components/ui/input";


  export default async function Home() {

  const product={name:"niceproduct",
     description:"this is a nice product", price:300}
  
  

 
  return (
   
      <Suspense fallback={<div>Loading...</div>}>
        <InputInline placeholder="Search for products..." />
       <div className="grid grid-cols-3 " >


      
     <CardImage>
      <CardTitle>{product.name}</CardTitle>
      <CardDescription>{product.description}</CardDescription>
      <CardAction>Buy Now for ${product.price}</CardAction>
      <button>Buy Now</button>
     </CardImage>
      <CardImage>
      <CardTitle>{product.name}</CardTitle>
      <CardDescription>{product.description}</CardDescription>
      <CardAction>Buy Now for ${product.price}</CardAction>
      <button>Buy Now</button>
     </CardImage>
             <CardImage>
      <CardTitle>{product.name}</CardTitle>
      <CardDescription>{product.description}</CardDescription>
      <CardAction>Buy Now for ${product.price}</CardAction>
      <button>Buy Now</button>
     </CardImage>

      </div>
     

      </Suspense>
  
  );
}
