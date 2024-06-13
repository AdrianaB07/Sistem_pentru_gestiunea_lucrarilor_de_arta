var tagList = [];

// Prepopulate the existing tags
if (typeof existingTags !== 'undefined' && existingTags !== null) {
    tagList = existingTags.split(',');
    updateTagList();
}

document.getElementById('addTagBtn').addEventListener('click', function () {
    var tagInput = document.getElementById('tagInput').value;
    if (tagInput.trim() !== "") {
        tagList.push(tagInput);
        updateTagList();
        document.getElementById('tagInput').value = '';
    }
});

function updateTagList() {
    var tagListElem = document.getElementById('tagList');
    tagListElem.innerHTML = '';
    tagList.forEach((tag, index) => {
        var li = document.createElement('li');
        li.textContent = tag;
        var removeBtn = document.createElement('button');
        removeBtn.textContent = 'x';
        removeBtn.addEventListener('click', function () {
            tagList.splice(index, 1);
            updateTagList();
        });
        li.appendChild(removeBtn);
        tagListElem.appendChild(li);
    });

    // Add hidden inputs to send the tagList to the server
    document.querySelectorAll('input[name="Tags"]').forEach(e => e.remove());
    tagList.forEach(tag => {
        var input = document.createElement('input');
        input.type = 'hidden';
        input.name = 'Tags';
        input.value = tag;
        tagListElem.appendChild(input);
    });
}

document.querySelector('input[type="file"]').addEventListener('change', function () {
    var reader = new FileReader();
    reader.onload = function (e) {
        var imgPreview = document.getElementById('uploadedImagePreview');
        imgPreview.style.display = 'block'; // Afișează imaginea
        imgPreview.src = e.target.result;
    }
    reader.readAsDataURL(this.files[0]);
});
