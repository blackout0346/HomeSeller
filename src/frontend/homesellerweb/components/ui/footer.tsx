

function FooterTitle({children, ...props}: React.ComponentProps<"label">) {
return (
<div className="">
<label {...props}/>
</div>
)}

function Footer(): React.ReactNode{
    return (
    <footer className="bg-black p-10 w-full" />
    )
}
export { Footer, FooterTitle }